using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using RealtimePPUR.Models;
using HitResult = osu.Game.Rulesets.Scoring.HitResult;

namespace RealtimePPUR.Services.PPCalculation;

public static class HitResultGenerator
{
    public static Dictionary<HitResult, int> FromHitResult(Models.HitResult hits, OsuGameMode mode)
    {
        return mode switch
        {
            OsuGameMode.Osu => new Dictionary<HitResult, int>
            {
                { HitResult.Great, hits.Hit300 },
                { HitResult.Ok, hits.Hit100 },
                { HitResult.Meh, hits.Hit50 },
                { HitResult.Miss, hits.HitMiss }
            },
            OsuGameMode.Taiko => new Dictionary<HitResult, int>
            {
                { HitResult.Great, hits.Hit300 },
                { HitResult.Ok, hits.Hit100 },
                { HitResult.Miss, hits.HitMiss }
            },
            OsuGameMode.Catch => new Dictionary<HitResult, int>
            {
                { HitResult.Great, hits.Hit300 },
                { HitResult.LargeTickHit, hits.Hit100 },
                { HitResult.SmallTickHit, hits.Hit50 },
                { HitResult.SmallTickMiss, hits.HitKatu },
                { HitResult.Miss, hits.HitMiss }
            },
            OsuGameMode.Mania => new Dictionary<HitResult, int>
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

    public static Dictionary<HitResult, int> ToSS(IBeatmap beatmap, OsuGameMode mode)
    {
        switch (mode)
        {
            case OsuGameMode.Osu:
                {
                    return new Dictionary<HitResult, int>
                    {
                        { HitResult.Great, beatmap.HitObjects.Count },
                        { HitResult.Ok, 0 },
                        { HitResult.Meh, 0 },
                        { HitResult.Miss, 0 }
                    };
                }

            case OsuGameMode.Taiko:
                {
                    int countGreat = GetMaxCombo(beatmap, mode);

                    return new Dictionary<HitResult, int>
                    {
                        { HitResult.Great, countGreat },
                        { HitResult.Ok, 0 },
                        { HitResult.Miss, 0 }
                    };
                }

            case OsuGameMode.Catch:
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

            case OsuGameMode.Mania:
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

    public static Dictionary<HitResult, int> ToLossMode(Dictionary<HitResult, int> statics, Models.HitResult hits, OsuGameMode mode)
    {
        return mode switch
        {
            OsuGameMode.Osu => new Dictionary<HitResult, int>
            {
                { HitResult.Great, statics[HitResult.Great] - hits.Hit100 - hits.Hit50 - hits.HitMiss },
                { HitResult.Ok, hits.Hit100 },
                { HitResult.Meh, hits.Hit50 },
                { HitResult.Miss, hits.HitMiss }
            },
            OsuGameMode.Taiko => new Dictionary<HitResult, int>
            {
                { HitResult.Great, statics[HitResult.Great] - hits.Hit100 - hits.HitMiss },
                { HitResult.Ok, hits.Hit100 },
                { HitResult.Miss, hits.HitMiss }
            },
            OsuGameMode.Catch => new Dictionary<HitResult, int>
            {
                { HitResult.Great, statics[HitResult.Great] - hits.Hit100 - hits.Hit50 - hits.HitKatu },
                { HitResult.LargeTickHit, hits.Hit100 },
                { HitResult.SmallTickHit, hits.Hit50 },
                { HitResult.SmallTickMiss, hits.HitKatu },
                { HitResult.Miss, hits.HitMiss }
            },
            OsuGameMode.Mania => new Dictionary<HitResult, int>
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

    // Copyright (c) 2022 Max Ohn
    // This code is borrowed from Bathbot(https://github.com/MaxOhn/Bathbot)
    // Bathbot is licensed under the ISC License. https://github.com/MaxOhn/Bathbot/blob/main/LICENSE
    public static Dictionary<HitResult, int> ToIfFC(IBeatmap beatmap, Models.HitResult hits, OsuGameMode mode)
    {
        switch (mode)
        {
            case OsuGameMode.Osu:
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

            case OsuGameMode.Taiko:
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

            case OsuGameMode.Catch:
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

            case OsuGameMode.Mania:
                {
                    return new Dictionary<HitResult, int>
                    {
                        [HitResult.Perfect] = beatmap.HitObjects.Count - hits.Hit300 - hits.HitKatu - hits.Hit100 - hits.Hit50 - hits.HitMiss,
                        [HitResult.Great] = hits.Hit300,
                        [HitResult.Good] = hits.HitKatu,
                        [HitResult.Ok] = hits.Hit100,
                        [HitResult.Meh] = hits.Hit50,
                        [HitResult.Miss] = 0
                    };
                }

            default:
                throw new ArgumentException("Invalid mode provided. Given mode: " + mode);
        }
    }

    public static int GetMaxCombo(IBeatmap beatmap, OsuGameMode mode)
    {
        return mode switch
        {
            OsuGameMode.Osu => beatmap.GetMaxCombo(),
            OsuGameMode.Taiko => beatmap.HitObjects.OfType<Hit>().Count(),
            OsuGameMode.Catch => beatmap.HitObjects.Count(h => h is Fruit) + beatmap.HitObjects.OfType<JuiceStream>().SelectMany(j => j.NestedHitObjects).Count(h => h is not TinyDroplet),
            OsuGameMode.Mania => beatmap.HitObjects.Count,
            _ => throw new ArgumentException("Invalid ruleset ID provided.")
        };
    }
    public static int CountTotalHitObjects(IBeatmap beatmap, OsuGameMode mode)
    {
        return mode switch
        {
            OsuGameMode.Osu => beatmap.HitObjects.Count,
            OsuGameMode.Taiko => GetMaxCombo(beatmap, mode),
            OsuGameMode.Catch => GetMaxCombo(beatmap, mode),
            OsuGameMode.Mania => GetMaxCombo(beatmap, mode),
            _ => throw new ArgumentException("Invalid ruleset ID provided.")
        };
    }
}
