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
        ProcessorWorkingBeatmap _workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);

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
            var staticsSS = GenerateHitResultsForSs(beatmap, mode);
            var scoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = 1,
                MaxCombo = GetMaxCombo(beatmap, mode),
                Statistics = staticsSS,
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
            data.IfFcHitResult = staticsSS;

            var statistics = GenerateHitResults(args.Accuracy / 100, beatmap, args.Misses, args.Mehs, args.Goods, mode, args);
            if (resultScreen)
            {
                var currentScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = args.Accuracy / 100,
                    MaxCombo = GetMaxCombo(beatmap, mode),
                    Statistics = statistics,
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
                    var staticsForCalcIfFc = calcIfFC(beatmap, hits, mode);
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
                    var performanceAttributesIFFC = performanceCalculatorIFFC?.Calculate(iffcScoreInfo, difficultyAttributesIFFC);
                    data.DifficultyAttributesIFFC = difficultyAttributesIFFC;
                    data.PerformanceAttributesIFFC = performanceAttributesIFFC;
                    data.IfFcHitResult = staticsForCalcIfFc;
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
                    Statistics = statistics,
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

        private static Dictionary<HitResult, int> GenerateHitResults(double accuracy, IBeatmap beatmap, int countMiss, int? countMeh, int? countGood, int mode, CalculateArgs args)
        {
            switch (mode)
            {
                case 0:
                    {
                        int countGreat;
                        int totalResultCount = beatmap.HitObjects.Count;

                        if (countMeh != null || countGood != null)
                        {
                            countGreat = totalResultCount - (countGood ?? 0) - (countMeh ?? 0) - countMiss;
                        }
                        else
                        {
                            int targetTotal = (int)Math.Round(accuracy * totalResultCount * 6);
                            int delta = targetTotal - (totalResultCount - countMiss);
                            countGreat = delta / 5;
                            countGood = delta % 5;
                            countMeh = totalResultCount - countGreat - countGood - countMiss;
                        }

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, countGreat },
                            { HitResult.Ok, countGood ?? 0 },
                            { HitResult.Meh, countMeh ?? 0 },
                            { HitResult.Miss, countMiss }
                        };
                    }

                case 1:
                    {
                        int totalResultCount = GetMaxCombo(beatmap, mode);

                        int countGreat;

                        if (countGood != null)
                        {
                            countGreat = (int)(totalResultCount - countGood - countMiss)!;
                        }
                        else
                        {
                            int targetTotal = (int)Math.Round(accuracy * totalResultCount * 2);

                            countGreat = targetTotal - (totalResultCount - countMiss);
                            countGood = totalResultCount - countGreat - countMiss;
                        }

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, countGreat },
                            { HitResult.Ok, (int)countGood },
                            { HitResult.Meh, 0 },
                            { HitResult.Miss, countMiss }
                        };
                    }

                case 2:
                    {
                        int maxCombo = GetMaxCombo(beatmap, mode);
                        int maxTinyDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<TinyDroplet>().Count());
                        int maxDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<Droplet>().Count()) - maxTinyDroplets;
                        int maxFruits = beatmap.HitObjects.Sum(h => h is Fruit ? 1 : (h as JuiceStream)?.NestedHitObjects.Count(n => n is Fruit) ?? 0);
                        int countDroplets = countGood ?? Math.Max(0, maxDroplets - countMiss);
                        int countFruits = maxFruits - (countMiss - (maxDroplets - countDroplets));
                        int countTinyDroplets = countMeh ?? (int)Math.Round(accuracy * (maxCombo + maxTinyDroplets)) - countFruits - countDroplets;
                        int countTinyMisses = maxTinyDroplets - countTinyDroplets;

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, countFruits },
                            { HitResult.LargeTickHit, countDroplets },
                            { HitResult.SmallTickHit, countTinyDroplets },
                            { HitResult.SmallTickMiss, countTinyMisses },
                            { HitResult.Miss, countMiss }
                        };
                    }

                case 3:
                    {
                        int totalHits = beatmap.HitObjects.Count + beatmap.HitObjects.Count(ho => ho is HoldNote);

                        if (countMeh != null || countGood != null || args.Greats != null)
                        {
                            int countPerfect = totalHits - (countMiss + (countMeh ?? 0) + (args.Oks) + (countGood ?? 0) + (args.Greats ?? 0));

                            return new Dictionary<HitResult, int>
                            {
                                [HitResult.Perfect] = countPerfect,
                                [HitResult.Great] = args.Greats ?? 0,
                                [HitResult.Good] = countGood ?? 0,
                                [HitResult.Ok] = args.Oks,
                                [HitResult.Meh] = countMeh ?? 0,
                                [HitResult.Miss] = countMiss
                            };
                        }

                        int targetTotal = (int)Math.Round(accuracy * totalHits * 6);
                        int remainingHits = totalHits - countMiss;
                        int delta = targetTotal - remainingHits;
                        args.Greats = Math.Min(delta / 5, remainingHits) / 2;
                        int perfects = args.Greats.Value;
                        delta -= (args.Greats.Value + perfects) * 5;
                        remainingHits -= (args.Greats.Value + perfects);
                        countGood = Math.Min(delta / 3, remainingHits);
                        delta -= countGood.Value * 3;
                        remainingHits -= countGood.Value;
                        args.Oks = delta;
                        countMeh = remainingHits;

                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Perfect, perfects },
                            { HitResult.Great, args.Greats.Value },
                            { HitResult.Ok, args.Oks },
                            { HitResult.Good, countGood.Value },
                            { HitResult.Meh, countMeh.Value },
                            { HitResult.Miss, countMiss }
                        };
                    }

                default:
                    throw new ArgumentException("Invalid ruleset ID provided.");
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

        private static Dictionary<HitResult, int> calcIfFC(IBeatmap beatmap, HitsResult hits, int mode)
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
                        var new100s = (int)Math.Ceiling(ratio * hits.HitMiss);
                        n300 += Math.Max(0, hits.HitMiss - new100s);
                        var n100 = hits.Hit100 + new100s;
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
                        var new100s = (int)Math.Ceiling(ratio * hits.HitMiss);
                        n300 += Math.Max(0, hits.HitMiss - new100s);
                        var n100 = hits.Hit100 + new100s;
                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, n300 },
                            { HitResult.Ok, n100 },
                            { HitResult.Miss, 0 }
                        };
                    }

                case 2:
                    {
                        var maxCombo = GetMaxCombo(beatmap, mode);
                        int maxTinyDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<TinyDroplet>().Count());
                        int maxDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<Droplet>().Count()) - maxTinyDroplets;
                        var missing = maxCombo - hits.Hit300 + hits.Hit100 + hits.HitMiss;
                        var missingFruits = Math.Max(0, missing - Math.Max(0, maxDroplets - hits.Hit100));
                        var missingDroplets = missing - missingFruits;
                        var nFruits = hits.Hit300 + missingFruits;
                        var nDroplets = hits.Hit100 + missingDroplets;
                        var nTinyDropletMisses = hits.HitKatu;
                        var nTinyDroplets = Math.Max(0, maxTinyDroplets - nTinyDropletMisses);
                        return new Dictionary<HitResult, int>
                        {
                            { HitResult.Great, nFruits },
                            { HitResult.LargeTickHit, nDroplets },
                            { HitResult.SmallTickHit, nTinyDroplets },
                            { HitResult.SmallTickMiss, nTinyDropletMisses },
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

        private double GetAccuracy(Dictionary<HitResult, int> statistics, int mode)
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
                    return 0;
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

        private static Beatmap readFromFile(string filename)
        {
            using var stream = File.OpenRead(filename);
            using var reader = new LineBufferedReader(stream);
            return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
        }

        public static ProcessorWorkingBeatmap FromFile(string file)
        {
            return new ProcessorWorkingBeatmap(readFromFile(file));
        }

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
        public int Misses { get; set; }
        public int? Mehs { get; set; }
        public int Goods { get; set; }
        public int Oks { get; set; }
        public int? Greats { get; set; }
        public int Combo { get; set; }
        public int Score { get; set; }
        public bool NoClassicMod { get; set; }
        public string[] Mods { get; set; } = Array.Empty<string>();
        public int? Time { get; set; }
    }
}
