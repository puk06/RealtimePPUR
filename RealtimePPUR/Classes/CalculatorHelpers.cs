using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Objects;

namespace RealtimePPUR.Classes
{
    internal class CalculatorHelpers
    {
        public static Ruleset SetRuleset(int mode)
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

        public static Mod[] GetMods(Ruleset ruleset, CalculateArgs args)
        {
            if (args.Mods.Length == 0) return ruleset.CreateAllMods().ToList().Where(m => m is OsuModClassic).ToArray();
            var availableMods = ruleset.CreateAllMods().ToList();
            var mods = args.Mods.Select(modString => availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase))).Where(newMod => newMod != null).ToArray();
            return mods.Append(new OsuModClassic()).ToArray();
        }

        public static Dictionary<HitResult, int> GenerateHitResultsForSs(IBeatmap beatmap, int mode)
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
                        return new Dictionary<HitResult, int>
                        {
                            [HitResult.Perfect] = beatmap.HitObjects.Count,
                            [HitResult.Great] = 0,
                            [HitResult.Good] = 0,
                            [HitResult.Ok] = 0,
                            [HitResult.Meh] = 0,
                            [HitResult.Miss] = 0
                        };
                    }

                default:
                    throw new ArgumentException("Invalid mode provided. Given mode: " + mode);
            }
        }

        public static Dictionary<HitResult, int> GenerateHitResultsForCurrent(HitsResult hits, int mode)
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
                _ => throw new ArgumentException("Invalid mode provided. Given mode: " + mode)
            };
        }

        public static Dictionary<HitResult, int> GenerateHitResultsForLossMode(Dictionary<HitResult, int> statics, HitsResult hits, int mode)
        {
            return mode switch
            {
                0 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, statics[HitResult.Great] - hits.Hit100 - hits.Hit50 - hits.HitMiss },
                    { HitResult.Ok, hits.Hit100 },
                    { HitResult.Meh, hits.Hit50 },
                    { HitResult.Miss, hits.HitMiss }
                },
                1 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, statics[HitResult.Great] - hits.Hit100 - hits.HitMiss },
                    { HitResult.Ok, hits.Hit100 },
                    { HitResult.Miss, hits.HitMiss }
                },
                2 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, statics[HitResult.Great] - hits.Hit100 - hits.Hit50 - hits.HitKatu },
                    { HitResult.LargeTickHit, hits.Hit100 },
                    { HitResult.SmallTickHit, hits.Hit50 },
                    { HitResult.SmallTickMiss, hits.HitKatu },
                    { HitResult.Miss, hits.HitMiss }
                },
                3 => new Dictionary<HitResult, int>
                {
                    [HitResult.Perfect] =
                        statics[HitResult.Perfect] - hits.Hit300 - hits.HitKatu - hits.Hit100 - hits.Hit50 -
                        hits.HitMiss,
                    [HitResult.Great] = hits.Hit300,
                    [HitResult.Good] = hits.HitKatu,
                    [HitResult.Ok] = hits.Hit100,
                    [HitResult.Meh] = hits.Hit50,
                    [HitResult.Miss] = hits.HitMiss
                },
                _ => throw new ArgumentException("Invalid mode provided. Given mode: " + mode)
            };
        }

        public static int CountTotalHitObjects(IBeatmap beatmap, int mode)
        {
            return mode switch
            {
                0 => beatmap.HitObjects.Count,
                1 => beatmap.HitObjects.OfType<Hit>().Count(),
                2 => beatmap.HitObjects.Count(h => h is Fruit) + beatmap.HitObjects.OfType<JuiceStream>()
                    .SelectMany(j => j.NestedHitObjects)
                    .Count(h => h is not TinyDroplet),
                3 => beatmap.HitObjects.Count,
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
        }

        public static Dictionary<HitResult, int> CalcIfFc(IBeatmap beatmap, HitsResult hits, int mode)
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
                        int totalObjects = GetMaxCombo(beatmap, mode);
                        int passedObjects = hits.Hit300 + hits.Hit100 + hits.HitMiss;
                        var juiceStreams = beatmap.HitObjects.OfType<JuiceStream>().ToList();
                        int maxTinyDroplets = juiceStreams.Sum(s => s.NestedHitObjects.OfType<TinyDroplet>().Count());
                        int maxDroplets = juiceStreams.Sum(s => s.NestedHitObjects.OfType<Droplet>().Count()) - maxTinyDroplets;
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
                    throw new ArgumentException("Invalid mode provided. Given mode: " + mode);
            }
        }

        public struct ModMultiplierModDivider
        {
            public double ModMultiplier;
            public double ModDivider;
        }

        public static ModMultiplierModDivider ModMultiplierModDividerCalculator(string[] mods)
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

        public static int ManiaScoreCalculator(IBeatmap beatmap, HitsResult hits, string[] mods, int currentScore)
        {
            const int hitValue = 320;
            const int hitBonusValue = 32;
            const int hitBonus = 2;
            const int hitPunishment = 0;

            int totalNotes = hits.HitGeki + hits.Hit300 + hits.HitKatu + hits.Hit100 + hits.Hit50 + hits.HitMiss;
            int objectCount = beatmap.HitObjects.Count;

            const int maxScore = 1000000;
            double bonus = 100;
            double baseScore = 0;
            double bonusScore = 0;
            var modValues = ModMultiplierModDividerCalculator(mods);

            const double hitValueRatio = hitValue / 320.0;
            const double hitBounsValueRatio = hitBonusValue / 320.0;
            double objectCountRatio = 0.5 / objectCount;
            double modMultiplier = modValues.ModMultiplier;

            for (int i = 0; i < totalNotes; i++)
            {
                bonus = Math.Max(0, Math.Min(100, (bonus + hitBonus - hitPunishment) / modValues.ModDivider));
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
                score = Math.Max((int)((maxScore * modMultiplier) - Math.Round((Math.Round(baseScore + bonusScore) - currentScore) / ratio)), 0);
            }

            if (double.IsNaN(score)) score = 0;

            return (int)Math.Round(score);
        }

        public static double GetAccuracy(IReadOnlyDictionary<HitResult, int> statistics, int mode)
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

                case 3:
                    {
                        double hits = (6 * statistics[HitResult.Perfect]) + (6 * statistics[HitResult.Great]) +
                                      (4 * statistics[HitResult.Good]) + (2 * statistics[HitResult.Ok]) +
                                      statistics[HitResult.Meh];
                        double total = 6 * (statistics[HitResult.Meh] + statistics[HitResult.Ok] +
                                            statistics[HitResult.Great] + statistics[HitResult.Miss] +
                                            statistics[HitResult.Perfect] + statistics[HitResult.Good]);

                        return hits / total;
                    }

                default:
                    throw new ArgumentException("Invalid mode provided. Given mode: " + mode);
            }
        }

        public static string GetCurrentRank(IReadOnlyDictionary<HitResult, int> statistics, int mode, string[] mods)
        {
            string rank = "Unknown";
            bool silver = mods.Contains("hd") || mods.Contains("fl");
            switch (mode)
            {
                case 0:
                    {
                        var h300 = statistics[HitResult.Great];
                        var h100 = statistics[HitResult.Ok];
                        var h50 = statistics[HitResult.Meh];
                        var h0 = statistics[HitResult.Miss];

                        int total = h300 + h100 + h50 + h0;
                        double r300 = (double)h300 / total;
                        double r50 = (double)h50 / total;

                        switch (r300)
                        {
                            case 1:
                                rank = silver ? "XH" : "X";
                                break;
                            case > 0.9 when r50 < 0.01 && h0 == 0:
                                rank = silver ? "SH" : "S";
                                break;
                            case > 0.8 when h0 == 0:
                            case > 0.9:
                                rank = "A";
                                break;
                            case > 0.7 when h0 == 0:
                            case > 0.8:
                                rank = "B";
                                break;
                            case > 0.6:
                                rank = "C";
                                break;
                            default:
                                rank = "D";
                                break;
                        }
                    }

                    break;

                case 1:
                    {
                        var h300 = statistics[HitResult.Great];
                        var h100 = statistics[HitResult.Ok];
                        var h0 = statistics[HitResult.Miss];
                        int total = h300 + h100 + h0;
                        double r300 = (double)h300 / total;

                        switch (r300)
                        {
                            case 1:
                                rank = silver ? "XH" : "X";
                                break;
                            case > 0.9 when h0 == 0:
                                rank = silver ? "SH" : "S";
                                break;
                            case > 0.8 when h0 == 0:
                            case > 0.9:
                                rank = "A";
                                break;
                            case > 0.7 when h0 == 0:
                            case > 0.8:
                                rank = "B";
                                break;
                            case > 0.6:
                                rank = "C";
                                break;
                            default:
                                rank = "D";
                                break;
                        }
                    }

                    break;

                case 2:
                    {
                        var h300 = statistics[HitResult.Great];
                        var h100 = statistics[HitResult.LargeTickHit];
                        var h50 = statistics[HitResult.SmallTickHit];
                        var katu = statistics[HitResult.SmallTickMiss];
                        var h0 = statistics[HitResult.Miss];
                        int total = h300 + h100 + h50 + h0 + katu;
                        double acc = total > 0 ? (h50 + h100 + h300) / (double)total : 1;

                        rank = acc switch
                        {
                            1 => silver ? "XH" : "X",
                            > 0.98 => silver ? "SH" : "S",
                            > 0.94 => "A",
                            > 0.9 => "B",
                            > 0.85 => "C",
                            _ => "D"
                        };
                    }

                    break;

                case 3:
                    {
                        var h300 = statistics[HitResult.Perfect];
                        var h100 = statistics[HitResult.Great];
                        var h50 = statistics[HitResult.Good];
                        var geki = statistics[HitResult.Ok];
                        var katu = statistics[HitResult.Meh];
                        var h0 = statistics[HitResult.Miss];
                        int total = h300 + h100 + h50 + h0 + geki + katu;
                        double acc = total > 0 ? ((h50 * 50) + (h100 * 100) + (katu * 200) + ((h300 + geki) * 300)) / (total * 300.0) : 1;

                        rank = acc switch
                        {
                            1 => silver ? "XH" : "X",
                            > 0.95 => silver ? "SH" : "S",
                            > 0.9 => "A",
                            > 0.8 => "B",
                            > 0.7 => "C",
                            _ => "D"
                        };
                    }

                    break;
            }

            return rank;
        }

        public static int GetMaxCombo(IBeatmap beatmap, int mode)
        {
            return mode switch
            {
                0 => beatmap.GetMaxCombo(),
                1 => beatmap.HitObjects.OfType<Hit>().Count(),
                2 => beatmap.HitObjects.Count(h => h is Fruit) + beatmap.HitObjects.OfType<JuiceStream>()
                    .SelectMany(j => j.NestedHitObjects)
                    .Count(h => h is not TinyDroplet),
                3 => beatmap.HitObjects.Count,
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
        }
    }
}
