using osu.Game.Rulesets.Difficulty.Skills;

namespace RealtimePPUR.Classes
{
    public interface IExtendedDifficultyCalculator
    {
        Skill[] GetSkills();
    }
}
