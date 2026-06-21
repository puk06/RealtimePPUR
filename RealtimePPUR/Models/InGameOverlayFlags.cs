using System;

namespace RealtimePPUR.Models;

[Flags]
public enum InGameOverlayValues
{
    None = 0,
    StarRatings = 1 << 0,
    SSPerformancePoint = 1 << 1,
    CurrentPerformancePoint = 1 << 2,
    CurrentAccuracy = 1 << 3,
    CurrentHits = 1 << 4,
    LossModeHits = 1 << 5,
    IfFCHits = 1 << 6,
    UnstableRate = 1 << 7,
    OffsetHelp = 1 << 8,
    AverageError = 1 << 9,
    SongProgress = 1 << 10,
    HealthPercentage = 1 << 11,
    Score = 1 << 12,
    Combo = 1 << 13,
    RemainingNotes = 1 << 14
}
