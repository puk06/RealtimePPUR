using osu.Game.Rulesets.Difficulty;

namespace RealtimePPUR.Models;

internal class MapDifficultyAttributes
{
    internal string[] Mods { get; set; } = [];
    internal DifficultyAttributes DifficultyAttributes { get; set; } = new DifficultyAttributes();
}
