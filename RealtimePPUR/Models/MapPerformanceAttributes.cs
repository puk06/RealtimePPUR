using osu.Game.Rulesets.Difficulty;

namespace RealtimePPUR.Models;

internal class MapPerformanceAttributes
{
    internal string[] Mods { get; set; } = [];
    internal PerformanceAttributes? PerformanceAttributes { get; set; }
}
