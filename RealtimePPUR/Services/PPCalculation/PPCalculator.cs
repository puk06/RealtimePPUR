using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using RealtimePPUR.Models;
using HitResult = osu.Game.Rulesets.Scoring.HitResult;
using static RealtimePPUR.Utils.OsuBeatmapUtils;

namespace RealtimePPUR.Services.PPCalculation;

public static class PPCalculator
{
    public static readonly Dictionary<OsuGameMode, Ruleset> RulesetDictionary = new()
    {
        { OsuGameMode.Osu, new OsuRuleset() },
        { OsuGameMode.Taiko, new TaikoRuleset() },
        { OsuGameMode.Catch, new CatchRuleset() },
        { OsuGameMode.Mania, new ManiaRuleset() }
    };

    private static readonly Dictionary<OsuGameMode, PerformanceCalculator> PerformanceCalculatorDictionary = new()
    {
        { OsuGameMode.Osu, RulesetDictionary[OsuGameMode.Osu].CreatePerformanceCalculator()! },
        { OsuGameMode.Taiko, RulesetDictionary[OsuGameMode.Taiko].CreatePerformanceCalculator()! },
        { OsuGameMode.Catch, RulesetDictionary[OsuGameMode.Catch].CreatePerformanceCalculator()! },
        { OsuGameMode.Mania, RulesetDictionary[OsuGameMode.Mania].CreatePerformanceCalculator()! }
    };

    public static void CalculateMapAttributes(PerformanceCalculationContext ctx, OsuBetmapInfo osuBeatmapInfo, SimplifiedAttributes simplifiedAttributes)
    {
        if (ctx.GameMode == OsuGameMode.None) return;

        var ruleset = RulesetDictionary[ctx.GameMode];

        var workingBeatmap = osuBeatmapInfo.ProcessorWorkingBeatmap;
        if (workingBeatmap == null) return;

        var playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, ctx.Mods);
        if (playableBeatmap == null) return;

        var difficultyCalculator = ruleset.CreateDifficultyCalculator(workingBeatmap);
        var performanceCalculator = PerformanceCalculatorDictionary[ctx.GameMode];

        var statistics = HitResultGenerator.ToSS(playableBeatmap, ctx.GameMode);
        var maxCombo = GetMaxCombo(playableBeatmap, ctx.GameMode);

        var scoreInfo = new ScoreInfo(playableBeatmap.BeatmapInfo, ruleset.RulesetInfo)
        {
            Accuracy = 1,
            MaxCombo = maxCombo,
            Statistics = statistics,
            Mods = ctx.Mods
        };

        var difficultyAttributes = difficultyCalculator.Calculate(ctx.Mods);
        var performanceAttibutes = performanceCalculator.Calculate(scoreInfo, difficultyAttributes);

        simplifiedAttributes.MapDifficultyAttributes = difficultyAttributes;
        simplifiedAttributes.MapPerformanceAttributes = performanceAttibutes;

        simplifiedAttributes.TotalHitObjectsCount = CountTotalHitObjects(playableBeatmap, ctx.GameMode);
    }

    public static void Calculate(PerformanceCalculationContext ctx, OsuBetmapInfo osuBeatmapInfo, SimplifiedAttributes simplifiedAttributes, Models.HitResult hitResult)
    {
        if (ctx.GameMode == OsuGameMode.None) return;
        if (simplifiedAttributes.MapDifficultyAttributes == null || simplifiedAttributes.MapPerformanceAttributes == null) CalculateMapAttributes(ctx, osuBeatmapInfo, simplifiedAttributes);
        if (simplifiedAttributes.MapDifficultyAttributes == null || simplifiedAttributes.MapPerformanceAttributes == null) return;

        var ruleset = RulesetDictionary[ctx.GameMode];

        var workingBeatmap = osuBeatmapInfo.ProcessorWorkingBeatmap;
        if (workingBeatmap == null) return;

        var playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, ctx.Mods);
        if (playableBeatmap == null) return;

        var performanceCalculator = PerformanceCalculatorDictionary[ctx.GameMode];

        var maxCombo = GetMaxCombo(playableBeatmap, ctx.GameMode);
        var statisticsSs = HitResultGenerator.ToSS(playableBeatmap, ctx.GameMode);
        var statisticsCurrent = HitResultGenerator.FromHitResult(hitResult, ctx.GameMode);
        var accuracy = GetAccuracy(statisticsCurrent, ctx.GameMode);

        simplifiedAttributes.CurrentStarRating = simplifiedAttributes.MapDifficultyAttributes.StarRating;
        simplifiedAttributes.CurrentPerformancePoint = simplifiedAttributes.MapPerformanceAttributes.Total;
        simplifiedAttributes.CurrentHitResults = statisticsCurrent;
        simplifiedAttributes.CurrentAccuracy = accuracy;

        if (ctx.IsResultScreen || ctx.IsPlaying)
        {
            var staticsForCalcIfFc = HitResultGenerator.ToIfFC(playableBeatmap, hitResult, ctx.GameMode);
            var iffcAccuracy = GetAccuracy(staticsForCalcIfFc, ctx.GameMode);
            var iffcScoreInfo = new ScoreInfo(playableBeatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = iffcAccuracy,
                MaxCombo = maxCombo,
                Statistics = staticsForCalcIfFc,
                Mods = ctx.Mods
            };

            var performanceAttributesIffc = performanceCalculator.Calculate(iffcScoreInfo, simplifiedAttributes.MapDifficultyAttributes);
            simplifiedAttributes.IfFCPerformancePoint = performanceAttributesIffc.Total;
            simplifiedAttributes.IfFCHitResults = staticsForCalcIfFc;
            simplifiedAttributes.IfFCAccuracy = iffcAccuracy;
        }

        if (ctx.IsResultScreen)
        {
            var resultScoreInfo = new ScoreInfo(playableBeatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = accuracy,
                MaxCombo = ctx.Combo,
                Statistics = statisticsCurrent,
                Mods = ctx.Mods,
                TotalScore = ctx.Score
            };

            var performanceAttributesResult = performanceCalculator.Calculate(resultScoreInfo, simplifiedAttributes.MapDifficultyAttributes);
            simplifiedAttributes.CurrentPerformancePoint = performanceAttributesResult.Total;

            return;
        }

        if (!ctx.IsPlaying) return;

        // CURRENT
        Beatmap beatmapCurrent = new()
        {
            HitObjects = playableBeatmap.HitObjects.Where(h => h.StartTime <= ctx.AudioTime).ToList(),
            ControlPointInfo = workingBeatmap.Beatmap.ControlPointInfo,
            BeatmapInfo = workingBeatmap.Beatmap.BeatmapInfo,
        };

        var workingBeatmapCurrent = new ProcessorWorkingBeatmap(beatmapCurrent);
        var difficultyCalculatorCurrent = ruleset.CreateDifficultyCalculator(workingBeatmapCurrent);
        var difficultyAttributesCurrent = difficultyCalculatorCurrent.Calculate(ctx.Mods);
        var currentScoreInfo = new ScoreInfo(playableBeatmap.BeatmapInfo, ruleset.RulesetInfo)
        {
            Accuracy = accuracy,
            MaxCombo = ctx.Combo,
            Statistics = statisticsCurrent,
            Mods = ctx.Mods,
            TotalScore = ctx.Score
        };
        var currentPerformanceAttributes = performanceCalculator.Calculate(currentScoreInfo, difficultyAttributesCurrent);

        simplifiedAttributes.CurrentStarRating = difficultyAttributesCurrent.StarRating;
        simplifiedAttributes.CurrentPerformancePoint = currentPerformanceAttributes.Total;
        simplifiedAttributes.CurrentHitResults = statisticsCurrent;

        // LOSS MODE
        if (ctx.GameMode == OsuGameMode.Taiko || ctx.GameMode == OsuGameMode.Mania)
        {
            var staticsLoss = HitResultGenerator.ToLossMode(statisticsSs, hitResult, ctx.GameMode);
            var lossModeAccuracy = GetAccuracy(staticsLoss, ctx.GameMode);
            var lossScoreInfo = new ScoreInfo(playableBeatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = lossModeAccuracy,
                MaxCombo = ctx.Combo,
                Statistics = staticsLoss,
                Mods = ctx.Mods,
                TotalScore = ctx.Score
            };

            var performanceAttributesLossMode = performanceCalculator.Calculate(lossScoreInfo, simplifiedAttributes.MapDifficultyAttributes);
            simplifiedAttributes.LossModePerformancePoint = performanceAttributesLossMode.Total;
            simplifiedAttributes.LossModeHitResults = staticsLoss;
            simplifiedAttributes.LossModeAccuracy = lossModeAccuracy;
        }
    }

    public static double GetAccuracy(IReadOnlyDictionary<HitResult, int> statistics, OsuGameMode mode)
    {
        switch (mode)
        {
            case OsuGameMode.Osu:
                {
                    var countGreat = statistics[HitResult.Great];
                    var countGood = statistics[HitResult.Ok];
                    var countMeh = statistics[HitResult.Meh];
                    var countMiss = statistics[HitResult.Miss];
                    var total = countGreat + countGood + countMeh + countMiss;

                    var acc = (double)((6 * countGreat) + (2 * countGood) + countMeh) / (6 * total);

                    return double.IsNaN(acc) ? 0 : acc;
                }

            case OsuGameMode.Taiko:
                {
                    var countGreat = statistics[HitResult.Great];
                    var countGood = statistics[HitResult.Ok];
                    var countMiss = statistics[HitResult.Miss];
                    var total = countGreat + countGood + countMiss;

                    var acc = (double)((2 * countGreat) + countGood) / (2 * total);

                    return double.IsNaN(acc) ? 0 : acc;
                }

            case OsuGameMode.Catch:
                {
                    double hits = statistics[HitResult.Great] + statistics[HitResult.LargeTickHit] + statistics[HitResult.SmallTickHit];
                    double total = hits + statistics[HitResult.Miss] + statistics[HitResult.SmallTickMiss];

                    var acc = hits / total;

                    return double.IsNaN(acc) ? 0 : acc;
                }

            case OsuGameMode.Mania:
                {
                    double hits =
                        (6 * statistics[HitResult.Perfect]) +
                        (6 * statistics[HitResult.Great]) +
                        (4 * statistics[HitResult.Good]) +
                        (2 * statistics[HitResult.Ok]) +
                        statistics[HitResult.Meh];

                    double total = 6 * (
                        statistics[HitResult.Meh] +
                        statistics[HitResult.Ok] +
                        statistics[HitResult.Great] +
                        statistics[HitResult.Miss] +
                        statistics[HitResult.Perfect] +
                        statistics[HitResult.Good]
                    );

                    var acc = hits / total;

                    return double.IsNaN(acc) ? 0 : acc;
                }

            default: return 0;
        }
    }
}
