using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Scoring;
using System.Collections.Generic;

namespace RealtimePPUR.Classes
{
    public class BeatmapData
    {
        public DifficultyAttributes CurrentDifficultyAttributes { get; set; }
        public PerformanceAttributes CurrentPerformanceAttributes { get; set; }
        public DifficultyAttributes DifficultyAttributes { get; set; }
        public PerformanceAttributes PerformanceAttributes { get; set; }
        public DifficultyAttributes DifficultyAttributesIffc { get; set; }
        public PerformanceAttributes PerformanceAttributesIffc { get; set; }
        public Dictionary<HitResult, int> IfFcHitResult { get; set; }
        public int ExpectedManiaScore { get; set; }
    }
}
