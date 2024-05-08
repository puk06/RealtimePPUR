using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace RealtimePPUR
{
    public class PpCalculator(string file, int mode)
    {
        private Ruleset _ruleset = SetRuleset(mode);
        private ProcessorWorkingBeatmap _workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);

        public void SetMap(string file, int givenmode)
        {
            _workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);
            _ruleset = SetRuleset(givenmode);
            mode = givenmode;
        }

        public void SetMode(int givenmode)
        {
            _ruleset = SetRuleset(givenmode);
            mode = givenmode;
        }

        private static Ruleset SetRuleset(int mode)
        {
            return mode switch
            {
                0 => new OsuRuleset(),
                1 => new TaikoRuleset(),
                2 => new CatchRuleset(),
                3 => new ManiaRuleset(),
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
        }

        private static Mod[] GetMods(Ruleset ruleset, CalculateArgs args)
        {
            if (args.Mods.Length == 0) return Array.Empty<Mod>();
            var availableMods = ruleset.CreateAllMods().ToList();
            return args.Mods.Select(modString => availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase))).Where(newMod => newMod != null).ToArray();
        }

        public BeatmapData Calculate(CalculateArgs args, bool playing, bool resultScreen, HitsResult hits)
        {
            var data = new BeatmapData();
            var mods = args.NoClassicMod ? GetMods(_ruleset, args) : LegacyHelper.FilterDifficultyAdjustmentMods(_workingBeatmap.BeatmapInfo, _ruleset, GetMods(_ruleset, args));
            var beatmap = _workingBeatmap.GetPlayableBeatmap(_ruleset.RulesetInfo, mods);
            var staticsSs = GenerateHitResultsForSs(beatmap, mode);
            var scoreInfo = new ScoreInfo(beatmap.BeatmapInfo, _ruleset.RulesetInfo)
            {
                Accuracy = 1,
                MaxCombo = GetMaxCombo(beatmap, mode),
                Statistics = staticsSs,
                Mods = mods
            };
            var difficultyCalculator = _ruleset.CreateDifficultyCalculator(_workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);
            var performanceCalculator = _ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);

            data.DifficultyAttributes = difficultyAttributes;
            data.PerformanceAttributes = performanceAttributes;
            data.CurrentDifficultyAttributes = difficultyAttributes;
            data.CurrentPerformanceAttributes = performanceAttributes;
            data.DifficultyAttributesIffc = difficultyAttributes;
            data.PerformanceAttributesIffc = performanceAttributes;
            data.IfFcHitResult = staticsSs;
            data.ExpectedManiaScore = 0;

            var statisticsCurrent = GenerateHitResultsForCurrent(hits, mode);

            if (resultScreen)
            {
                var resultScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, _ruleset.RulesetInfo)
                {
                    Accuracy = args.Accuracy / 100,
                    MaxCombo = args.Combo,
                    Statistics = statisticsCurrent,
                    Mods = mods
                };

                var performanceAttributesResult = performanceCalculator?.Calculate(resultScoreInfo, difficultyAttributes);

                data.CurrentDifficultyAttributes = difficultyAttributes;
                data.CurrentPerformanceAttributes = performanceAttributesResult;

                return data;
            }

            if (!playing) return data;
            {
                if (mode != 3)
                {
                    var staticsForCalcIfFc = CalcIfFc(beatmap, hits, mode);

                    var iffcScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, _ruleset.RulesetInfo)
                    {
                        Accuracy = GetAccuracy(staticsForCalcIfFc, mode),
                        MaxCombo = GetMaxCombo(beatmap, mode),
                        Statistics = staticsForCalcIfFc,
                        Mods = mods
                    };

                    var performanceAttributesIffc = performanceCalculator?.Calculate(iffcScoreInfo, difficultyAttributes);

                    data.DifficultyAttributesIffc = difficultyAttributes;
                    data.PerformanceAttributesIffc = performanceAttributesIffc;
                    data.IfFcHitResult = staticsForCalcIfFc;
                }
                else
                {
                    data.ExpectedManiaScore = ManiaScoreCalculator(beatmap, hits, args.Mods, args.Score);
                }

                if (args.PplossMode && mode is 1 or 3)
                {
                    var staticsLoss = GenerateHitResultsForLossMode(beatmap, hits, mode);

                    var lossScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, _ruleset.RulesetInfo)
                    {
                        Accuracy = GetAccuracy(staticsLoss, mode),
                        MaxCombo = args.Combo,
                        Statistics = staticsLoss,
                        Mods = mods
                    };

                    var performanceAttributesCurrent = performanceCalculator?.Calculate(lossScoreInfo, difficultyAttributes);

                    data.CurrentDifficultyAttributes = difficultyAttributes;
                    data.CurrentPerformanceAttributes = performanceAttributesCurrent;
                }
                else
                {
                    Beatmap beatmapCurrent = new();
                    var hitObjects = _workingBeatmap.Beatmap.HitObjects.Where(h => h.StartTime <= args.Time).ToList();
                    beatmapCurrent.HitObjects.AddRange(hitObjects);
                    beatmapCurrent.ControlPointInfo = _workingBeatmap.Beatmap.ControlPointInfo;
                    beatmapCurrent.BeatmapInfo = _workingBeatmap.Beatmap.BeatmapInfo;

                    var currentScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, _ruleset.RulesetInfo)
                    {
                        Accuracy = args.Accuracy / 100,
                        MaxCombo = args.Combo,
                        Statistics = statisticsCurrent,
                        Mods = mods,
                        TotalScore = args.Score
                    };

                    var workingBeatmapCurrent = new ProcessorWorkingBeatmap(beatmapCurrent);
                    var difficultyCalculatorCurrent = _ruleset.CreateDifficultyCalculator(workingBeatmapCurrent);
                    var difficultyAttributesCurrent = difficultyCalculatorCurrent.Calculate(mods);
                    var performanceCalculatorCurrent = _ruleset.CreatePerformanceCalculator();
                    var performanceAttributesCurrent = performanceCalculatorCurrent?.Calculate(currentScoreInfo, difficultyAttributesCurrent);

                    data.CurrentDifficultyAttributes = difficultyAttributesCurrent;
                    data.CurrentPerformanceAttributes = performanceAttributesCurrent;
                }

                return data;
            }
        }

        private static Dictionary<HitResult, int> GenerateHitResultsForSs(IBeatmap beatmap, int mode)
        {
            switch (mode)
            {
                case 0:
                    {
                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, beatmap.HitObjects.Count },
                            { HitResult.Ok, 0 },
                            { HitResult.Meh, 0 },
                            { HitResult.Miss, 0 }
                        };
                    }

                case 1:
                    {
                        int countGreat = GetMaxCombo(beatmap, mode);

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, countGreat },
                            { HitResult.Ok, 0 },
                            { HitResult.Miss, 0 }
                        };
                    }

                case 2:
                    {
                        int maxCombo = GetMaxCombo(beatmap, mode);
                        var juiceStreams = beatmap.HitObjects.OfType<JuiceStream>().ToList();
                        int maxTinyDroplets = juiceStreams.Sum(s => s.NestedHitObjects.OfType<TinyDroplet>().Count());
                        int maxDroplets = juiceStreams.Sum(s => s.NestedHitObjects.OfType<Droplet>().Count()) - maxTinyDroplets;
                        int maxFruits = beatmap.HitObjects.Sum(h => h is Fruit ? 1 : (h as JuiceStream)?.NestedHitObjects.Count(n => n is Fruit) ?? 0);
                        int countDroplets = Math.Max(0, maxDroplets);
                        int countFruits = maxFruits + (maxDroplets - countDroplets);
                        int countTinyDroplets = maxCombo + maxTinyDroplets - countFruits - countDroplets;
                        int countTinyMisses = maxTinyDroplets - countTinyDroplets;

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, countFruits },
                            { HitResult.LargeTickHit, countDroplets },
                            { HitResult.SmallTickHit, countTinyDroplets },
                            { HitResult.SmallTickMiss, countTinyMisses },
                            { HitResult.Miss, 0 }
                        };
                    }

                case 3:
                    {
                        int totalHits = beatmap.HitObjects.Count + beatmap.HitObjects.Count(ho => ho is HoldNote);

                        return new Dictionary<HitResult, int>
                        {
                            [HitResult.Perfect] = totalHits,
                            [HitResult.Great] = 0,
                            [HitResult.Good] = 0,
                            [HitResult.Ok] = 0,
                            [HitResult.Meh] = 0,
                            [HitResult.Miss] = 0
                        };
                    }

                default:
                    throw new ArgumentException("Invalid mode provided.");
            }
        }

        private static Dictionary<HitResult, int> GenerateHitResultsForCurrent(HitsResult hits, int mode)
        {
            return mode switch
            {
                0 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, hits.Hit300 },
                    { HitResult.Ok, hits.Hit100 },
                    { HitResult.Meh, hits.Hit50 },
                    { HitResult.Miss, hits.HitMiss }
                },
                1 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, hits.Hit300 },
                    { HitResult.Ok, hits.Hit100 },
                    { HitResult.Miss, hits.HitMiss }
                },
                2 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, hits.Hit300 },
                    { HitResult.LargeTickHit, hits.Hit100 },
                    { HitResult.SmallTickHit, hits.Hit50 },
                    { HitResult.SmallTickMiss, hits.HitKatu },
                    { HitResult.Miss, hits.HitMiss }
                },
                3 => new Dictionary<HitResult, int>
                {
                    { HitResult.Perfect, hits.HitGeki },
                    { HitResult.Great, hits.Hit300 },
                    { HitResult.Good, hits.HitKatu },
                    { HitResult.Ok, hits.Hit100 },
                    { HitResult.Meh, hits.Hit50 },
                    { HitResult.Miss, hits.HitMiss }
                },
                _ => throw new ArgumentException("Invalid mode provided.")
            };
        }

        private static Dictionary<HitResult, int> GenerateHitResultsForLossMode(IBeatmap beatmap, HitsResult hits, int mode)
        {
            switch (mode)
            {
                case 1:
                    {
                        int countGreat = GetMaxCombo(beatmap, mode);

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, countGreat - hits.Hit100 - hits.HitMiss },
                            { HitResult.Ok, hits.Hit100 },
                            { HitResult.Miss, hits.HitMiss }
                        };
                    }

                case 3:
                    {
                        int totalHits = beatmap.HitObjects.Count + beatmap.HitObjects.Count(ho => ho is HoldNote);

                        return new Dictionary<HitResult, int>
                        {
                            [HitResult.Perfect] = totalHits - hits.Hit300 - hits.HitKatu - hits.Hit100 - hits.Hit50 - hits.HitMiss,
                            [HitResult.Great] = hits.Hit300,
                            [HitResult.Good] = hits.HitKatu,
                            [HitResult.Ok] = hits.Hit100,
                            [HitResult.Meh] = hits.Hit50,
                            [HitResult.Miss] = hits.HitMiss
                        };
                    }

                default:
                    throw new ArgumentException("Invalid mode provided.");
            }
        }

        private static Dictionary<HitResult, int> CalcIfFc(IBeatmap beatmap, HitsResult hits, int mode)
        {
            switch (mode)
            {
                case 0:
                    {
                        var objects = beatmap.HitObjects.Count;
                        var passedObjects = hits.Hit300 + hits.Hit100 + hits.Hit50 + hits.HitMiss;
                        var n300 = hits.Hit300 + Math.Max(0, objects - passedObjects);
                        var countHits = objects - hits.HitMiss;
                        var ratio = 1.0 - (double)n300 / countHits;
                        var new100S = (int)Math.Ceiling(ratio * hits.HitMiss);
                        n300 += Math.Max(0, hits.HitMiss - new100S);
                        var n100 = hits.Hit100 + new100S;
                        var n50 = hits.Hit50;

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, n300 },
                            { HitResult.Ok, n100 },
                            { HitResult.Meh, n50 },
                            { HitResult.Miss, 0 }
                        };
                    }

                case 1:
                    {
                        var objects = beatmap.HitObjects.OfType<Hit>().Count();
                        var passedObjects = hits.Hit300 + hits.Hit100 + hits.HitMiss;
                        var n300 = hits.Hit300 + Math.Max(0, objects - passedObjects);
                        var countHits = objects - hits.HitMiss;
                        var ratio = 1.0 - ((double)n300 / countHits);
                        var new100S = (int)Math.Ceiling(ratio * hits.HitMiss);
                        n300 += Math.Max(0, hits.HitMiss - new100S);
                        var n100 = hits.Hit100 + new100S;

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, n300 },
                            { HitResult.Ok, n100 },
                            { HitResult.Miss, 0 }
                        };
                    }

                case 2:
                    {
                        int totalObjects = GetMaxCombo(beatmap, mode);
                        int passedObjects = hits.Hit300 + hits.Hit100 + hits.HitMiss;
                        int maxTinyDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<TinyDroplet>().Count());
                        int maxDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<Droplet>().Count()) - maxTinyDroplets;
                        int missing = totalObjects - passedObjects;
                        int missingFruits = Math.Max(0, missing - Math.Max(0, maxDroplets - hits.Hit100));
                        int missingDroplets = missing - missingFruits;
                        int nFruits = hits.Hit300 + missingFruits;
                        int nDroplets = hits.Hit100 + missingDroplets;
                        int nTinyDropletMisses = hits.HitKatu;
                        int nTinyDroplets = Math.Max(0, maxTinyDroplets - nTinyDropletMisses);

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, nFruits },
                            { HitResult.LargeTickHit, nDroplets },
                            { HitResult.SmallTickHit, nTinyDroplets},
                            { HitResult.SmallTickMiss, nTinyDropletMisses },
                            { HitResult.Miss, 0 }
                        };
                    }

                default:
                    throw new ArgumentException("Invalid mode provided.");
            }
        }

        private struct ModMultiplierModDivider
        {
            public double ModMultiplier;
            public double ModDivider;
        }

        private static ModMultiplierModDivider ModMultiplierModDividerCalculator(string[] mods)
        {
            double modMultiplier = 1;
            double modDivider = 1;

            if (mods.Contains("ez")) modMultiplier *= 0.5;
            if (mods.Contains("nf")) modMultiplier *= 0.5;
            if (mods.Contains("ht")) modMultiplier *= 0.5;
            if (mods.Contains("hr")) modDivider /= 1.08;
            if (mods.Contains("dt")) modDivider /= 1.1;
            if (mods.Contains("nc")) modDivider /= 1.1;
            if (mods.Contains("fi")) modDivider /= 1.06;
            if (mods.Contains("hd")) modDivider /= 1.06;
            if (mods.Contains("fl")) modDivider /= 1.06;

            return new ModMultiplierModDivider { ModMultiplier = modMultiplier, ModDivider = modDivider };
        }

        private static int ManiaScoreCalculator(IBeatmap beatmap, HitsResult hits, string[] mods, int currentScore)
        {
            const int HitValue = 320;
            const int HitBonusValue = 32;
            const int HitBonus = 2;
            const int HitPunishment = 0;

            int totalNotes = hits.HitGeki + hits.Hit300 + hits.HitKatu + hits.Hit100 + hits.Hit50 + hits.HitMiss;
            int objectCount = beatmap.HitObjects.Count + beatmap.HitObjects.Count(ho => ho is HoldNote);

            const int maxScore = 1000000;
            double bonus = 100;
            double baseScore = 0;
            double bonusScore = 0;
            var modValues = ModMultiplierModDividerCalculator(mods);

            double hitValueRatio = HitValue / 320.0;
            double hitBounsValueRatio = HitBonusValue / 320;
            double objectCountRatio = 0.5 / objectCount;
            double modMultiplier = modValues.ModMultiplier;

            for (int i = 0; i < totalNotes; i++)
            {
                bonus = Math.Max(0, Math.Min(100, (bonus + HitBonus - HitPunishment) / modValues.ModDivider));
                baseScore += maxScore * modMultiplier * objectCountRatio * hitValueRatio;
                bonusScore += maxScore * modMultiplier * objectCountRatio * hitBounsValueRatio * Math.Sqrt(bonus);
            }

            double ratio = (double)totalNotes / objectCount;
            double score = 0;
            if (totalNotes == hits.HitGeki)
            {
                score = (int)(maxScore * modMultiplier);
            }
            else if (totalNotes != hits.HitMiss)
            {
                score = Math.Max((int)(maxScore * modMultiplier - Math.Round((Math.Round(baseScore + bonusScore) - currentScore) / ratio)), 0);
            }

            if (double.IsNaN(score)) score = 0;

            return (int)Math.Round(score);
        }

        private static double GetAccuracy(IReadOnlyDictionary<HitResult, int> statistics, int mode)
        {
            switch (mode)
            {
                case 0:
                    {
                        var countGreat = statistics[HitResult.Great];
                        var countGood = statistics[HitResult.Ok];
                        var countMeh = statistics[HitResult.Meh];
                        var countMiss = statistics[HitResult.Miss];
                        var total = countGreat + countGood + countMeh + countMiss;

                        return (double)(6 * countGreat + (2 * countGood) + countMeh) / (6 * total);
                    }

                case 1:
                    {
                        var countGreat = statistics[HitResult.Great];
                        var countGood = statistics[HitResult.Ok];
                        var countMiss = statistics[HitResult.Miss];
                        var total = countGreat + countGood + countMiss;

                        return (double)(2 * countGreat + countGood) / (2 * total);
                    }

                case 2:
                    {
                        double hits = statistics[HitResult.Great] + statistics[HitResult.LargeTickHit] + statistics[HitResult.SmallTickHit];
                        double total = hits + statistics[HitResult.Miss] + statistics[HitResult.SmallTickMiss];

                        return hits / total;
                    }

                case 3:
                    {
                        double hits = 6 * statistics[HitResult.Perfect] + 6 * statistics[HitResult.Great] +
                                      4 * statistics[HitResult.Good] + 2 * statistics[HitResult.Ok] +
                                      statistics[HitResult.Meh];
                        double total = 6 * (statistics[HitResult.Meh] + statistics[HitResult.Ok] +
                                            statistics[HitResult.Great] + statistics[HitResult.Miss] +
                                            statistics[HitResult.Perfect] + statistics[HitResult.Good]);

                        return hits / total;
                    }

                default:
                    throw new ArgumentException("Invalid mode provided.");
            }
        }

        private static int GetMaxCombo(IBeatmap beatmap, int mode)
        {
            return mode switch
            {
                0 => beatmap.GetMaxCombo(),
                1 => beatmap.HitObjects.OfType<Hit>().Count(),
                2 => beatmap.HitObjects.Count(h => h is Fruit) + beatmap.HitObjects.OfType<JuiceStream>().SelectMany(j => j.NestedHitObjects).Count(h => h is not TinyDroplet),
                3 => 0,
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
        }
    }

    public class ProcessorWorkingBeatmap : WorkingBeatmap
    {
        private readonly Beatmap _beatmap;

        public ProcessorWorkingBeatmap(Beatmap beatmap)
            : base(beatmap.BeatmapInfo, null)
        {
            _beatmap = beatmap;
            beatmap.BeatmapInfo.Ruleset = LegacyHelper.GetRulesetFromLegacyId(beatmap.BeatmapInfo.Ruleset.OnlineID).RulesetInfo;
        }

        private static Beatmap ReadFromFile(string filename)
        {
            using var stream = File.OpenRead(filename);
            using var reader = new LineBufferedReader(stream);
            return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
        }

        public static ProcessorWorkingBeatmap FromFile(string file) => new(ReadFromFile(file));

        protected override IBeatmap GetBeatmap() => _beatmap;
        public override Texture GetBackground() => null!;
        protected override Track GetBeatmapTrack() => null!;
        protected override ISkin GetSkin() => null!;
        public override Stream? GetStream(string storagePath) => null;
    }

    public static class LegacyHelper
    {
        public static Ruleset GetRulesetFromLegacyId(int id)
        {
            return id switch
            {
                0 => new OsuRuleset(),
                1 => new TaikoRuleset(),
                2 => new CatchRuleset(),
                3 => new ManiaRuleset(),
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
        }

        private const LegacyMods KeyMods = LegacyMods.Key1 | LegacyMods.Key2 | LegacyMods.Key3 | LegacyMods.Key4 |
                                            LegacyMods.Key5 | LegacyMods.Key6 | LegacyMods.Key7 | LegacyMods.Key8
                                            | LegacyMods.Key9 | LegacyMods.KeyCoop;

        private static LegacyMods MaskRelevantMods(LegacyMods mods, bool isConvertedBeatmap, int rulesetId)
        {
            LegacyMods relevantMods =
                LegacyMods.DoubleTime | LegacyMods.HalfTime | LegacyMods.HardRock | LegacyMods.Easy;

            switch (rulesetId)
            {
                case 0:
                    if ((mods & LegacyMods.Flashlight) > 0)
                        relevantMods |= LegacyMods.Flashlight | LegacyMods.Hidden | LegacyMods.TouchDevice;
                    else
                        relevantMods |= LegacyMods.Flashlight | LegacyMods.TouchDevice;
                    break;

                case 3:
                    if (isConvertedBeatmap)
                        relevantMods |= KeyMods;
                    break;
            }

            return mods & relevantMods;
        }

        private static LegacyMods ConvertToLegacyDifficultyAdjustmentMods(BeatmapInfo beatmapInfo, Ruleset ruleset,
            Mod?[] mods)
        {
            var legacyMods = ruleset.ConvertToLegacyMods(mods!);

            // mods that are not represented in `LegacyMods` (but we can approximate them well enough with others)
            if (mods.Any(mod => mod is ModDaycore))
                legacyMods |= LegacyMods.HalfTime;

            return MaskRelevantMods(legacyMods, ruleset.RulesetInfo.OnlineID != beatmapInfo.Ruleset.OnlineID,
                ruleset.RulesetInfo.OnlineID);
        }

        public static Mod?[] FilterDifficultyAdjustmentMods(BeatmapInfo beatmapInfo, Ruleset ruleset, Mod?[] mods)
            => ruleset.ConvertFromLegacyMods(ConvertToLegacyDifficultyAdjustmentMods(beatmapInfo, ruleset, mods))
                .ToArray();
    }

    public class BeatmapData
    {
        public DifficultyAttributes? CurrentDifficultyAttributes { get; set; }
        public PerformanceAttributes? CurrentPerformanceAttributes { get; set; }
        public DifficultyAttributes? DifficultyAttributes { get; set; }
        public PerformanceAttributes? PerformanceAttributes { get; set; }
        public DifficultyAttributes? DifficultyAttributesIffc { get; set; }
        public PerformanceAttributes? PerformanceAttributesIffc { get; set; }
        public Dictionary<HitResult, int> IfFcHitResult { get; set; }
        public int ExpectedManiaScore { get; set; }
    }

    public class HitsResult
    {
        public int HitGeki { get; set; }
        public int Hit300 { get; set; }
        public int HitKatu { get; set; }
        public int Hit100 { get; set; }
        public int Hit50 { get; set; }
        public int HitMiss { get; set; }
        public int Combo { get; set; }
        public int Score { get; set; }
        public HitsResult Clone()
        {
            return new()
            {
                HitGeki = HitGeki,
                Hit300 = Hit300,
                HitKatu = HitKatu,
                Hit100 = Hit100,
                Hit50 = Hit50,
                HitMiss = HitMiss,
                Combo = Combo,
                Score = Score
            };
        }
        public bool Equals(HitsResult other)
        {
            return HitGeki == other.HitGeki && Hit300 == other.Hit300 && HitKatu == other.HitKatu && Hit100 == other.Hit100 && Hit50 == other.Hit50 && HitMiss == other.HitMiss && Combo == other.Combo && Score == other.Score;
        }
        public bool IsEmpty()
        {
            return HitGeki == 0 && Hit300 == 0 && HitKatu == 0 && Hit100 == 0 && Hit50 == 0 && HitMiss == 0 && Combo == 0 && Score == 0;
        }
    }

    public class CalculateArgs
    {
        public double Accuracy { get; set; } = 100;
        public int Combo { get; set; }
        public int Score { get; set; }
        public bool NoClassicMod { get; set; }
        public string[] Mods { get; set; } = Array.Empty<string>();
        public int? Time { get; set; }
        public bool PplossMode { get; set; }
    }
}
