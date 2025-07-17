// Copyright(c) 2019 ppy Pty Ltd <contact@ppy.sh>.
// This code is borrowed from osu-tools(https://github.com/ppy/osu-tools)
// osu-tools is licensed under the MIT License. https://github.com/ppy/osu-tools/blob/master/LICENCE

using osu.Game.Rulesets.Difficulty.Skills;

namespace RealtimePPUR.Models;

internal interface IExtendedDifficultyCalculator
{
    Skill[] GetSkills();
}

