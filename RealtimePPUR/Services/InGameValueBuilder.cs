using System;
using System.Collections.Generic;
using System.Text;
using RealtimePPUR.Services.PPCalculation;

namespace RealtimePPUR.Services;

public static class InGameValueBuilder
{
    private delegate string InGameValueRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator);
    private static readonly Dictionary<int, InGameValueRowBuilder> InGameValueRowBuilders = new()
    {
        { 1, StarRatingRowBuilder },
        { 2, MapPerformanceRowBuilder },
        { 3, PerformanceRowBuilder },
        { 4, AccuracyRowBuilder },
        { 5, HitResultRowBuilder }
    };

    public static string Build(MemoryData memory, RealtimePPCalculator calculator, int[] valueList)
    {
        StringBuilder valueBuilder = new();

        bool firstValueAdded = false;
        foreach (int value in valueList)
        {
            if (!InGameValueRowBuilders.ContainsKey(value)) continue;

            if (firstValueAdded) valueBuilder.Append(Environment.NewLine);
            valueBuilder.Append(InGameValueRowBuilders[value](memory, calculator));

            firstValueAdded = true;
        }

        return valueBuilder.ToString();
    }
    
    private static string GenerateInGameValueRow(string title, string value)
    {
        return $"{title}: {value}";
    }

    #region Builders
    private static string StarRatingRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;

        var currentStars = attributes.CurrentStarRating;
        var mapStars = attributes.MapDifficultyAttributes?.StarRating ?? 0;

        return GenerateInGameValueRow(
            "SR",
            $"{currentStars:F2} / {mapStars:F2}"
        );
    }

    private static string MapPerformanceRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;

        var mapPerformance = attributes.MapPerformanceAttributes?.Total ?? 0;

        return GenerateInGameValueRow(
            "SSPP",
            $"{mapPerformance:F2}pp"
        );
    }

    private static string PerformanceRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;

        var ifFc = attributes.IfFCPerformancePoint;
        var lossMode = attributes.LossModePerformancePoint;
        var current = attributes.CurrentPerformancePoint;

        var mode = calculator.CurrentCalculationGameMode;
        if (mode == Models.OsuGameMode.Taiko || mode == Models.OsuGameMode.Mania)
        {
            return GenerateInGameValueRow(
                "PP",
                $"{current:F2} / {lossMode:F2}pp"
            );
        }
        else
        {
            return GenerateInGameValueRow(
                "PP",
                $"{current:F2} / {ifFc:F2}pp"
            );
        }
    }

    private static string AccuracyRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;
        var mode = calculator.CurrentCalculationGameMode;

        var current = PPCalculator.GetAccuracy(attributes.CurrentHitResults, mode) * 100;

        if (mode == Models.OsuGameMode.Taiko || mode == Models.OsuGameMode.Mania)
        {
            var lossMode = PPCalculator.GetAccuracy(attributes.LossModeHitResults, mode) * 100;
            return GenerateInGameValueRow(
                "ACC",
                $"{current:F2} / {lossMode:F2}%"
            );
        }
        else
        {
            return GenerateInGameValueRow(
                "ACC",
                $"{current:F2}%"
            );
        }
    }

    private static string HitResultRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;
        var hitResults = attributes.CurrentHitResults;

        var hitResultsString = string.Join('/', hitResults.Values);

        return GenerateInGameValueRow(
            "Hits",
            hitResultsString
        );
    }
    #endregion
}
