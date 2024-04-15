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
    public class PPCalculator(string file, int mode)
    {
        private Ruleset ruleset = SetRuleset(mode);
        private ProcessorWorkingBeatmap _workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);

        public void SetMap(string file, int _mode)
        {
            _workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);
            ruleset = SetRuleset(_mode);
            mode = _mode;
        }

        public void SetMode(int _mode)
        {
            ruleset = SetRuleset(_mode);
            mode = _mode;
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
            var mods = args.NoClassicMod ? GetMods(ruleset, args) : LegacyHelper.FilterDifficultyAdjustmentMods(_workingBeatmap.BeatmapInfo, ruleset, GetMods(ruleset, args));
            var beatmap = _workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);
            var staticsSs = GenerateHitResultsForSs(beatmap, mode);
            var scoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = 1,
                MaxCombo = GetMaxCombo(beatmap, mode),
                Statistics = staticsSs,
                Mods = mods
            };
            if (mods is { Length: > 0 }) scoreInfo.Mods = mods;
            var difficultyCalculator = ruleset.CreateDifficultyCalculator(_workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);
            var performanceCalculator = ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);
            data.DifficultyAttributes = difficultyAttributes;
            data.PerformanceAttributes = performanceAttributes;
            data.CurrentDifficultyAttributes = difficultyAttributes;
            data.CurrentPerformanceAttributes = performanceAttributes;
            data.DifficultyAttributesIFFC = difficultyAttributes;
            data.PerformanceAttributesIFFC = performanceAttributes;
            data.IfFcHitResult = staticsSs;
            data.ExpectedManiaScore = 0;

            var statisticsCurrent = GenerateHitResultsForCurrent(hits, mode);
            if (resultScreen)
            {
                var currentScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = args.Accuracy / 100,
                    MaxCombo = GetMaxCombo(beatmap, mode),
                    Statistics = statisticsCurrent,
                    Mods = mods
                };
                var difficultyCalculatorCurrent = ruleset.CreateDifficultyCalculator(_workingBeatmap);
                var difficultyAttributesCurrent = difficultyCalculatorCurrent.Calculate(mods);
                var performanceCalculatorCurrent = ruleset.CreatePerformanceCalculator();
                var performanceAttributesCurrent = performanceCalculatorCurrent?.Calculate(currentScoreInfo, difficultyAttributesCurrent);
                data.CurrentDifficultyAttributes = difficultyAttributesCurrent;
                data.CurrentPerformanceAttributes = performanceAttributesCurrent;
                return data;
            }

            if (!playing) return data;
            {
                if (mode != 3)
                {
                    var staticsForCalcIfFc = CalcIfFc(beatmap, hits, mode);
                    var iffcScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                    {
                        Accuracy = GetAccuracy(staticsForCalcIfFc, mode),
                        MaxCombo = GetMaxCombo(beatmap, mode),
                        Statistics = staticsForCalcIfFc,
                        Mods = mods
                    };
                    var difficultyCalculatorIFFC = ruleset.CreateDifficultyCalculator(_workingBeatmap);
                    var difficultyAttributesIFFC = difficultyCalculatorIFFC.Calculate(mods);
                    var performanceCalculatorIFFC = ruleset.CreatePerformanceCalculator();
                    var performanceAttributesIFFC =
                        performanceCalculatorIFFC?.Calculate(iffcScoreInfo, difficultyAttributesIFFC);
                    data.DifficultyAttributesIFFC = difficultyAttributesIFFC;
                    data.PerformanceAttributesIFFC = performanceAttributesIFFC;
                    data.IfFcHitResult = staticsForCalcIfFc;
                }
                else
                {
                    data.ExpectedManiaScore = ManiaScoreCalculator(beatmap, hits, args.Mods, args.Score);
                }

                Beatmap beatmapCurrent = new Beatmap();
                var hitObjects = _workingBeatmap.Beatmap.HitObjects.Where(h => h.StartTime <= args.Time).ToList();
                beatmapCurrent.HitObjects.AddRange(hitObjects);
                beatmapCurrent.ControlPointInfo = _workingBeatmap.Beatmap.ControlPointInfo;
                beatmapCurrent.BeatmapInfo = _workingBeatmap.Beatmap.BeatmapInfo;
                var currentScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = args.Accuracy / 100,
                    MaxCombo = args.Combo,
                    Statistics = statisticsCurrent,
                    Mods = mods,
                    TotalScore = args.Score
                };
                var workingBeatmapCurrent = new ProcessorWorkingBeatmap(beatmapCurrent);
                var difficultyCalculatorCurrent = ruleset.CreateDifficultyCalculator(workingBeatmapCurrent);
                var difficultyAttributesCurrent = difficultyCalculatorCurrent.Calculate(mods);
                var performanceCalculatorCurrent = ruleset.CreatePerformanceCalculator();
                var performanceAttributesCurrent = performanceCalculatorCurrent?.Calculate(currentScoreInfo, difficultyAttributesCurrent);
                data.CurrentDifficultyAttributes = difficultyAttributesCurrent;
                data.CurrentPerformanceAttributes = performanceAttributesCurrent;

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
                        int totalResultCount = GetMaxCombo(beatmap, mode);
                        int targetTotal = totalResultCount * 2;
                        var countGreat = targetTotal - totalResultCount;

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, countGreat },
                            { HitResult.Ok, 0 },
                            { HitResult.Meh, 0 },
                            { HitResult.Miss, 0 }
                        };
                    }

                case 2:
                    {
                        int maxCombo = GetMaxCombo(beatmap, mode);
                        int maxTinyDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<TinyDroplet>().Count());
                        int maxDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<Droplet>().Count()) - maxTinyDroplets;
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
                    throw new ArgumentException("Invalid ruleset ID provided.");
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
            var judgement = new
            {
                HitValue = 320,
                HitBonusValue = 32,
                HitBonus = 2,
                HitPunishment = 0
            };

            int totalNotes = hits.HitGeki + hits.Hit300 + hits.HitKatu + hits.Hit100 + hits.Hit50 + hits.HitMiss;
            int objectCount = beatmap.HitObjects.Count + beatmap.HitObjects.Count(ho => ho is HoldNote);

            const int maxScore = 1000000;
            double bonus = 100;
            double baseScore = 0;
            double bonusScore = 0;
            var modValues = ModMultiplierModDividerCalculator(mods);

            for (int i = 0; i < totalNotes; i++)
            {
                bonus = Math.Max(0, Math.Min(100, (bonus + judgement.HitBonus - judgement.HitPunishment) / modValues.ModDivider));
                baseScore += maxScore * modValues.ModMultiplier * 0.5 / objectCount * judgement.HitValue / 320.0;
                bonusScore += maxScore * modValues.ModMultiplier * 0.5 / objectCount * judgement.HitBonusValue * Math.Sqrt(bonus) / 320.0;
            }

            double ratio = (double)totalNotes / objectCount;
            double score = 0;
            if (totalNotes == hits.HitGeki)
            {
                score = (int)(maxScore * modValues.ModMultiplier);
            }
            else if (totalNotes != hits.HitMiss)
            {
                score = Math.Max((int)(maxScore * modValues.ModMultiplier - Math.Round((Math.Round(baseScore + bonusScore) - currentScore) / ratio)), 0);
            }
            if (double.IsNaN(score)) score = 0;

            return (int)Math.Round(score);
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
                        var ratio = 1.0 - ((double)n300 / countHits);
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
                        int maxCombo = GetMaxCombo(beatmap, mode);
                        int maxTinyDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<TinyDroplet>().Count());
                        int maxDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<Droplet>().Count()) - maxTinyDroplets;
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

                default:
                    return new Dictionary<HitResult, int>
                    {
                        { HitResult.Great, 0 },
                        { HitResult.Ok, 0 },
                        { HitResult.Good, 0 },
                        { HitResult.Meh, 0 },
                        { HitResult.Miss, 0 }
                    };
            }
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

                        return (double)((6 * countGreat) + (2 * countGood) + countMeh) / (6 * total);
                    }

                case 1:
                    {
                        var countGreat = statistics[HitResult.Great];
                        var countGood = statistics[HitResult.Ok];
                        var countMiss = statistics[HitResult.Miss];
                        var total = countGreat + countGood + countMiss;

                        return (double)((2 * countGreat) + countGood) / (2 * total);
                    }

                case 2:
                    {
                        double hits = statistics[HitResult.Great] + statistics[HitResult.LargeTickHit] + statistics[HitResult.SmallTickHit];
                        double total = hits + statistics[HitResult.Miss] + statistics[HitResult.SmallTickMiss];

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

        private const LegacyMods key_mods = LegacyMods.Key1 | LegacyMods.Key2 | LegacyMods.Key3 | LegacyMods.Key4 |
                                            LegacyMods.Key5 | LegacyMods.Key6 | LegacyMods.Key7 | LegacyMods.Key8
                                            | LegacyMods.Key9 | LegacyMods.KeyCoop;

        private static LegacyMods maskRelevantMods(LegacyMods mods, bool isConvertedBeatmap, int rulesetId)
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
                        relevantMods |= key_mods;
                    break;
            }

            return mods & relevantMods;
        }

        private static LegacyMods convertToLegacyDifficultyAdjustmentMods(BeatmapInfo beatmapInfo, Ruleset ruleset,
            Mod?[] mods)
        {
            var legacyMods = ruleset.ConvertToLegacyMods(mods!);

            // mods that are not represented in `LegacyMods` (but we can approximate them well enough with others)
            if (mods.Any(mod => mod is ModDaycore))
                legacyMods |= LegacyMods.HalfTime;

            return maskRelevantMods(legacyMods, ruleset.RulesetInfo.OnlineID != beatmapInfo.Ruleset.OnlineID,
                ruleset.RulesetInfo.OnlineID);
        }

        public static Mod?[] FilterDifficultyAdjustmentMods(BeatmapInfo beatmapInfo, Ruleset ruleset, Mod?[] mods)
            => ruleset.ConvertFromLegacyMods(convertToLegacyDifficultyAdjustmentMods(beatmapInfo, ruleset, mods))
                .ToArray();
    }

    public class BeatmapData
    {
        public DifficultyAttributes? CurrentDifficultyAttributes { get; set; }
        public PerformanceAttributes? CurrentPerformanceAttributes { get; set; }
        public DifficultyAttributes? DifficultyAttributes { get; set; }
        public PerformanceAttributes? PerformanceAttributes { get; set; }
        public DifficultyAttributes? DifficultyAttributesIFFC { get; set; }
        public PerformanceAttributes? PerformanceAttributesIFFC { get; set; }
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
    }

    public class CalculateArgs
    {
        public double Accuracy { get; set; } = 100;
        public int Combo { get; set; }
        public int Score { get; set; }
        public bool NoClassicMod { get; set; }
        public string[] Mods { get; set; } = Array.Empty<string>();
        public int? Time { get; set; }
    }
}
