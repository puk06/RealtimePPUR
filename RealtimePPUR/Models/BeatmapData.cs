using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Scoring;

namespace RealtimePPUR.Models;

internal class BeatmapData
{
    internal DifficultyAttributes? CurrentDifficultyAttributes { get; set; }
    internal PerformanceAttributes? CurrentPerformanceAttributes { get; set; }
    internal DifficultyAttributes? DifficultyAttributes { get; set; }
    internal PerformanceAttributes? PerformanceAttributes { get; set; }
    internal DifficultyAttributes? DifficultyAttributesIffc { get; set; }
    internal PerformanceAttributes? PerformanceAttributesIffc { get; set; }
    internal PerformanceAttributes? PerformanceAttributesLossMode { get; set; }
    internal Dictionary<HitResult, int> HitResults { get; set; } = [];
    internal Dictionary<HitResult, int> IfFcHitResult { get; set; } = [];
    internal Dictionary<HitResult, int> HitResultLossMode { get; set; } = [];
    internal int ExpectedManiaScore { get; set; }
    internal double CurrentBpm { get; set; }
    internal int TotalHitObjectCount { get; set; }
}
