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
    IfFCHits = 1 << 5,
    UnstableRate = 1 << 6,
    OffsetHelp = 1 << 7,
    AverageError = 1 << 8,
    SongProgress = 1 << 9,
    HealthPercentage = 1 << 10,
    Score = 1 << 11,
    RemainingNotes = 1 << 12
}
