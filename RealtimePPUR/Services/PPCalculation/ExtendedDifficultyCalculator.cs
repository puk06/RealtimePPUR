// Copyright(c) 2019 ppy Pty Ltd <contact@ppy.sh>.
// This code is borrowed from osu-tools(https://github.com/ppy/osu-tools)
// osu-tools is licensed under the MIT License. https://github.com/ppy/osu-tools/blob/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko.Difficulty;
using RealtimePPUR.Interfaces;
using RealtimePPUR.Models;

namespace RealtimePPUR.Services.PPCalculation;

internal static class ExtendedDifficultyCalculatorExtensions
{
    public static IExtendedDifficultyCalculator? GetExtendedDifficultyCalculator(this Ruleset ruleset, IWorkingBeatmap beatmap, OsuGameMode mode)
    {
        return mode switch
        {
            OsuGameMode.Osu => new ExtendedOsuDifficultyCalculator(ruleset.RulesetInfo, beatmap),
            OsuGameMode.Taiko => new ExtendedTaikoDifficultyCalculator(ruleset.RulesetInfo, beatmap),
            OsuGameMode.Catch => new ExtendedCatchDifficultyCalculator(ruleset.RulesetInfo, beatmap),
            OsuGameMode.Mania => new ExtendedManiaDifficultyCalculator(ruleset.RulesetInfo, beatmap),
            _ => null
        };
    }
}

internal class ExtendedOsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap) : OsuDifficultyCalculator(ruleset, beatmap), IExtendedDifficultyCalculator
{
    private Skill[] skills = [];
    public Skill[] GetSkills() => skills;

    protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills)
    {
        this.skills = skills;
        return base.CreateDifficultyAttributes(beatmap, mods, skills);
    }
}

internal class ExtendedTaikoDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap) : TaikoDifficultyCalculator(ruleset, beatmap), IExtendedDifficultyCalculator
{
    private Skill[] skills = [];
    public Skill[] GetSkills() => skills;

    protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills)
    {
        this.skills = skills;
        return base.CreateDifficultyAttributes(beatmap, mods, skills);
    }
}

internal class ExtendedCatchDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap) : CatchDifficultyCalculator(ruleset, beatmap), IExtendedDifficultyCalculator
{
    private Skill[] skills = [];
    public Skill[] GetSkills() => skills;

    protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills)
    {
        this.skills = skills;
        return base.CreateDifficultyAttributes(beatmap, mods, skills);
    }
}

internal class ExtendedManiaDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap) : ManiaDifficultyCalculator(ruleset, beatmap), IExtendedDifficultyCalculator
{
    private Skill[] skills = [];
    public Skill[] GetSkills() => skills;

    protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills)
    {
        this.skills = skills;
        return base.CreateDifficultyAttributes(beatmap, mods, skills);
    }
}
