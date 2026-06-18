using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;

namespace RealtimePPUR.Models;

public class SimplifiedPerformanceAttribute
{
    public Mod[] CalculatedMods { get; set; } = Array.Empty<Mod>();
    public double PerformancePoint { get; set; } = 0;
    public Dictionary<osu.Game.Rulesets.Scoring.HitResult, int> CalculatedHitResult { get; set; } = new();
}
