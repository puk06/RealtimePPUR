using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace RealtimePPUR.Classes
{
    public interface IExtendedDifficultyCalculator
    {
        Skill[] GetSkills();
        DifficultyHitObject[] GetDifficultyHitObjects(IBeatmap beatmap, double clockRate);
    }
}
