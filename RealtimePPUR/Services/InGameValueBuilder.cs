using System;
using System.Collections.Generic;
using System.Text;
using RealtimePPUR.Models;

namespace RealtimePPUR.Services;

public static class InGameValueBuilder
{
    private delegate string InGameValueRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator);
    private static readonly Dictionary<InGameOverlayValues, InGameValueRowBuilder> InGameValueRowBuilders = new()
    {
        { InGameOverlayValues.StarRatings, StarRatingRowBuilder },
        { InGameOverlayValues.SSPerformancePoint, MapPerformanceRowBuilder },
        { InGameOverlayValues.CurrentPerformancePoint, PerformanceRowBuilder },
        { InGameOverlayValues.CurrentAccuracy, AccuracyRowBuilder },
        { InGameOverlayValues.CurrentHits, HitResultRowBuilder },
        { InGameOverlayValues.LossModeHits, LossModeHitResultRowBuilder },
        { InGameOverlayValues.IfFCHits, IfFCHitResultRowBuilder },
        { InGameOverlayValues.UnstableRate, UnstableRateRowBuilder },
        { InGameOverlayValues.OffsetHelp, OffsetHelpRowBuilder },
        { InGameOverlayValues.AverageError, AverageErrorRowBuilder },
        { InGameOverlayValues.HealthPercentage, HealthPercentageRowBuilder },
        { InGameOverlayValues.Score, ScoreRowBuilder },
        { InGameOverlayValues.Combo, ComboRowBuilder },
        { InGameOverlayValues.RemainingNotes, RemainingNotesRowBuilder }
    };

    public static string Build(MemoryData memory, RealtimePPCalculator calculator, InGameOverlayValues flags)
    {
        StringBuilder valueBuilder = new();

        var firstValueAdded = false;
        for (int i = 0; i < InGameValueRowBuilders.Keys.Count; i++)
        {
            var bit = (InGameOverlayValues)(1 << i);
            if ((flags & bit) != bit || !InGameValueRowBuilders.ContainsKey(bit)) continue;

            if (firstValueAdded) valueBuilder.Append(Environment.NewLine);
            valueBuilder.Append(InGameValueRowBuilders[bit](memory, calculator));

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
        var settings = calculator.RuntimeSettings;

        var ifFc = attributes.IfFCPerformancePoint;
        var lossMode = attributes.LossModePerformancePoint;
        var current = attributes.CurrentPerformancePoint;
        
        var mode = calculator.CurrentCalculationGameMode;
        var isLossModeAvailable = mode == OsuGameMode.Taiko || mode == OsuGameMode.Mania;
        var isLossModeEnabled = settings.EnableLossMode;
        var isCurrentPpLossMode = isLossModeAvailable && isLossModeEnabled;

        if (isLossModeAvailable)
        {
            if (isCurrentPpLossMode)
            {
                return GenerateInGameValueRow(
                    "PP",
                    $"{lossMode:F2} / {ifFc:F2}pp"
                );
            }
            else
            {
                return GenerateInGameValueRow(
                    "PP",
                    $"{current:F2} / {lossMode:F2}pp"
                );
            }
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

        var current = attributes.CurrentAccuracy * 100;

        if (mode == OsuGameMode.Taiko || mode == OsuGameMode.Mania)
        {
            var lossMode = attributes.LossModeAccuracy * 100;
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
    private static string LossModeHitResultRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;
        var hitResults = attributes.LossModeHitResults;

        var hitResultsString = string.Join('/', hitResults.Values);

        return GenerateInGameValueRow(
            "LossHits",
            hitResultsString
        );
    }
    private static string IfFCHitResultRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;
        var hitResults = attributes.IfFCHitResults;

        var hitResultsString = string.Join('/', hitResults.Values);

        return GenerateInGameValueRow(
            "IFFCHits",
            hitResultsString
        );
    }
    private static string UnstableRateRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;
        double unstableRate = attributes.HitErrorInfo.UnstableRate;

        return GenerateInGameValueRow(
            "UR",
            unstableRate.ToString("F2")
        );
    }
    private static string OffsetHelpRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;
        double offsetHelp = Math.Round(attributes.HitErrorInfo.Average);

        return GenerateInGameValueRow(
            "OffsetHelp",
            offsetHelp.ToString("F0")
        );
    }
    private static string AverageErrorRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;
        var average = -attributes.HitErrorInfo.Average;

        return GenerateInGameValueRow(
            "AvgError",
            $"{average:F2}ms"
        );
    }
    private static string HealthPercentageRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var hp = memoryData.HealthPercentage / 2;

        return GenerateInGameValueRow(
            "HP",
            $"{hp:F1}%"
        );
    }
    private static string ScoreRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var score = memoryData.CurrentScore;

        return GenerateInGameValueRow(
            "Score",
            score.ToString()
        );
    }
    private static string ComboRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var combo = memoryData.CurrentCombo;
        var maxCombo = memoryData.CurrentMaxCombo;

        return GenerateInGameValueRow(
            "Combo",
            $"{combo} / {maxCombo}x"
        );
    }
    private static string RemainingNotesRowBuilder(MemoryData memoryData, RealtimePPCalculator calculator)
    {
        var attributes = calculator.CurrentAttributes;
        var total = attributes.TotalHitObjectsCount;

        var mode = calculator.CurrentCalculationGameMode;

        var hits = calculator.CurrentMemoryData.HitResult;
        int currentNotes = mode switch
        {
            OsuGameMode.Osu => hits.Hit300 + hits.Hit100 + hits.Hit50 + hits.HitMiss,
            OsuGameMode.Taiko => hits.Hit300 + hits.Hit100 + hits.HitMiss,
            OsuGameMode.Catch => hits.Hit300 + hits.Hit100 + hits.HitMiss,
            OsuGameMode.Mania => hits.HitGeki + hits.Hit300 + hits.HitKatu + hits.Hit100 + hits.Hit50 + hits.HitMiss,
            _ => 0
        };

        var remainingNotes = total - currentNotes;

        return GenerateInGameValueRow(
            "Notes",
            remainingNotes.ToString()
        );
    }
    #endregion
}
