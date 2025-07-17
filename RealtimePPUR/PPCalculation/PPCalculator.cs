using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Scoring;
using RealtimePPUR.Models;
using RealtimePPUR.Utils;

namespace RealtimePPUR.PPCalculation
{
    internal class PpCalculator(string file, int mode)
    {
        private Ruleset ruleset = CalculatorUtils.GetRuleset(mode);
        private ProcessorWorkingBeatmap workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);
        private List<TimedDifficultyAttributes>? currentDifficultyAttributes = null;
        private MapDifficultyAttributes? currentMapDifficultyAttributes = null;
        private MapPerformanceAttributes? currentMapPerformanceAttributes = null;
        private int totalHitObjectCount;

        internal void SetMap(string file, int givenmode)
        {
            ruleset = CalculatorUtils.GetRuleset(givenmode);
            mode = givenmode;

            workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);

            currentDifficultyAttributes = null;
            currentMapDifficultyAttributes = null;
            currentMapPerformanceAttributes = null;
        }

        internal void SetMode(int givenmode)
        {
            ruleset = CalculatorUtils.GetRuleset(givenmode);
            mode = givenmode;

            currentDifficultyAttributes = null;
            currentMapDifficultyAttributes = null;
            currentMapPerformanceAttributes = null;
        }

        internal BeatmapData Calculate(CalculateArgs args, bool playing, bool resultScreen, HitsResult hits)
        {
            var mods = CalculatorUtils.GetMods(ruleset, args);
            var beatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);

            var staticsSs = CalculatorUtils.GenerateHitResultsForSs(beatmap, mode);

            var difficultyAttributes = GetCurrentMapDifficultyAttributes(args, beatmap);
            var performanceAttributes = GetCurrentMapPerformanceAttributes(args, beatmap, difficultyAttributes);

            var data = new BeatmapData()
            {
                DifficultyAttributes = difficultyAttributes,
                PerformanceAttributes = performanceAttributes.PerformanceAttributes,
                CurrentDifficultyAttributes = difficultyAttributes,
                CurrentPerformanceAttributes = performanceAttributes.PerformanceAttributes,
                DifficultyAttributesIffc = difficultyAttributes,
                PerformanceAttributesIffc = performanceAttributes.PerformanceAttributes,
                PerformanceAttributesLossMode = performanceAttributes.PerformanceAttributes,
                IfFcHitResult = staticsSs,
                ExpectedManiaScore = 0,
                CurrentBpm = 0,
                TotalHitObjectCount = totalHitObjectCount
            };

            var statisticsCurrent = CalculatorUtils.GenerateHitResultsForCurrent(hits, mode);
            data.HitResults = statisticsCurrent;
            data.HitResultLossMode = statisticsCurrent;

            if (resultScreen)
            {
                var resultScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = args.Accuracy / 100,
                    MaxCombo = args.Combo,
                    Statistics = statisticsCurrent,
                    Mods = mods,
                    TotalScore = args.Score
                };
                var performanceCalculator = ruleset.CreatePerformanceCalculator();
                var performanceAttributesResult = performanceCalculator?.Calculate(resultScoreInfo, difficultyAttributes);
                data.CurrentPerformanceAttributes = performanceAttributesResult;

                var staticsForCalcIfFc = CalculatorUtils.CalcIfFc(beatmap, hits, mode);
                var iffcScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = CalculatorUtils.GetAccuracy(staticsForCalcIfFc, mode),
                    MaxCombo = CalculatorUtils.GetMaxCombo(beatmap, mode),
                    Statistics = staticsForCalcIfFc,
                    Mods = mods,
                    TotalScore = args.Score
                };
                var performanceAttributesIffc = performanceCalculator?.Calculate(iffcScoreInfo, difficultyAttributes);
                data.PerformanceAttributesIffc = performanceAttributesIffc;
                data.IfFcHitResult = staticsForCalcIfFc;

                return data;
            }

            if (!playing) return data;
            {
                var performanceCalculator = ruleset.CreatePerformanceCalculator();
                DifficultyAttributes difficultyAttributesCurrent;

                if (args.CalculateBeforePlaying)
                {
                    difficultyAttributesCurrent = GetCurrentDifficultyAttributes(args, args.Time);
                }
                else
                {
                    Beatmap beatmapCurrent = new();
                    var hitObjects = workingBeatmap.Beatmap.HitObjects.Where(h => h.StartTime <= args.Time).ToList();
                    beatmapCurrent.HitObjects.AddRange(hitObjects);
                    beatmapCurrent.ControlPointInfo = workingBeatmap.Beatmap.ControlPointInfo;
                    beatmapCurrent.BeatmapInfo = workingBeatmap.Beatmap.BeatmapInfo;

                    var workingBeatmapCurrent = new ProcessorWorkingBeatmap(beatmapCurrent);
                    var difficultyCalculatorCurrent = ruleset.CreateDifficultyCalculator(workingBeatmapCurrent);
                    difficultyAttributesCurrent = difficultyCalculatorCurrent.Calculate(mods);
                }

                var currentScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = CalculatorUtils.GetAccuracy(statisticsCurrent, mode),
                    MaxCombo = args.Combo,
                    Statistics = statisticsCurrent,
                    Mods = mods,
                    TotalScore = args.Score
                };

                var performanceCalculatorCurrent = ruleset.CreatePerformanceCalculator();
                if (performanceCalculatorCurrent == null)
                {
                    LogUtils.DebugLogger("PerformanceCalculator is null, returning empty BeatmapData.");
                    return data;
                }
                var performanceAttributesCurrent = performanceCalculatorCurrent.Calculate(currentScoreInfo, difficultyAttributesCurrent);

                data.CurrentDifficultyAttributes = difficultyAttributesCurrent;
                data.CurrentPerformanceAttributes = performanceAttributesCurrent;

                // Calculation Loss Mode PP
                if (mode is 1 or 3)
                {
                    var staticsLoss = CalculatorUtils.GenerateHitResultsForLossMode(staticsSs, hits, mode);
                    data.HitResultLossMode = staticsLoss;

                    var lossScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                    {
                        Accuracy = CalculatorUtils.GetAccuracy(staticsLoss, mode),
                        MaxCombo = args.Combo,
                        Statistics = staticsLoss,
                        Mods = mods,
                        TotalScore = args.Score
                    };

                    var performanceAttributesLossMode = performanceCalculator?.Calculate(lossScoreInfo, difficultyAttributes);
                    data.PerformanceAttributesLossMode = performanceAttributesLossMode;
                    if (mode == 3) data.ExpectedManiaScore = CalculatorUtils.ManiaScoreCalculator(beatmap, hits, args.Mods, args.Score);
                }

                var staticsForCalcIfFc = CalculatorUtils.CalcIfFc(beatmap, hits, mode);
                data.IfFcHitResult = staticsForCalcIfFc;

                var iffcScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = CalculatorUtils.GetAccuracy(staticsForCalcIfFc, mode),
                    MaxCombo = CalculatorUtils.GetMaxCombo(beatmap, mode),
                    Statistics = staticsForCalcIfFc,
                    Mods = mods,
                    TotalScore = args.Score
                };

                var performanceAttributesIffc = performanceCalculator?.Calculate(iffcScoreInfo, difficultyAttributes);
                data.PerformanceAttributesIffc = performanceAttributesIffc;

                // Get Current BPM
                var timingPoints = beatmap.ControlPointInfo.TimingPoints;
                TimingControlPoint? lastTimingPoint = null;

                foreach (var tp in timingPoints)
                {
                    if (tp is not null && tp.Time <= args.Time)
                    {
                        lastTimingPoint = tp;
                    }
                }

                if (lastTimingPoint == null) return data;
                data.CurrentBpm = lastTimingPoint.BPM;

                return data;
            }
        }

        private DifficultyAttributes GetCurrentDifficultyAttributes(CalculateArgs args, int? time)
        {
            time ??= 0;
            currentDifficultyAttributes ??= CalculateAllTimedDifficulties(args);
            var difficultyAttributes = currentDifficultyAttributes.LastOrDefault(d => d.Time <= time);
            if (difficultyAttributes != null) return difficultyAttributes.Attributes;
            difficultyAttributes = currentDifficultyAttributes.FirstOrDefault();
            return difficultyAttributes?.Attributes ?? new DifficultyAttributes();
        }

        private DifficultyAttributes GetCurrentMapDifficultyAttributes(CalculateArgs args, IBeatmap beatmap)
        {
            if (currentMapDifficultyAttributes != null && !currentMapDifficultyAttributes.Mods.SequenceEqual(args.Mods))
            {
                LogUtils.DebugLogger("Mods changed, recalculating Map DifficultyAttributes...");
                currentMapDifficultyAttributes = null;
                currentDifficultyAttributes = null;
            }

            currentMapDifficultyAttributes ??= CalculateMapDifficultyAttributes(args, beatmap);
            return currentMapDifficultyAttributes.DifficultyAttributes;
        }

        private List<TimedDifficultyAttributes> CalculateAllTimedDifficulties(CalculateArgs args)
        {
            LogUtils.DebugLogger($"Calculating All DifficultyAttributes...");
            var currentTime = DateTime.Now;

            var mods = CalculatorUtils.GetMods(ruleset, args);
            var difficultyCalculator = ruleset.CreateDifficultyCalculator(workingBeatmap);
            var difficultyAttributes = difficultyCalculator.CalculateTimed(mods);

            var elapsed = DateTime.Now - currentTime;
            LogUtils.DebugLogger($"Calculated All DifficultyAttributes! (Total Time: " + elapsed.Milliseconds + " milliseconds)");

            return difficultyAttributes;
        }

        private MapDifficultyAttributes CalculateMapDifficultyAttributes(CalculateArgs args, IBeatmap beatmap)
        {
            LogUtils.DebugLogger("Calculating Map DifficultyAttributes...");
            var currentTime = DateTime.Now;

            var mods = CalculatorUtils.GetMods(ruleset, args);
            var difficultyCalculator = ruleset.CreateDifficultyCalculator(workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);

            var elapsed = DateTime.Now - currentTime;
            LogUtils.DebugLogger("Calculated Map DifficultyAttributes! (Total Time: " + elapsed.Milliseconds + " milliseconds)");

            totalHitObjectCount = CalculatorUtils.CountTotalHitObjects(beatmap, mode);
            LogUtils.DebugLogger("Total HitObject Count: " + totalHitObjectCount);

            return new MapDifficultyAttributes
            {
                Mods = args.Mods,
                DifficultyAttributes = difficultyAttributes
            };
        }

        private MapPerformanceAttributes GetCurrentMapPerformanceAttributes(CalculateArgs args, IBeatmap beatmap, DifficultyAttributes difficultyAttributes)
        {
            if (currentMapPerformanceAttributes != null && !currentMapPerformanceAttributes.Mods.SequenceEqual(args.Mods))
            {
                LogUtils.DebugLogger("Mods changed, recalculating Map PerformanceAttributes...");
                currentMapPerformanceAttributes = null;
            }

            currentMapPerformanceAttributes ??= CalculateMapPerformanceAttributes(args, beatmap, difficultyAttributes);
            return currentMapPerformanceAttributes;
        }

        private MapPerformanceAttributes CalculateMapPerformanceAttributes(CalculateArgs args, IBeatmap beatmap, DifficultyAttributes difficultyAttributes)
        {
            LogUtils.DebugLogger("Calculating Map PerformanceAttributes...");
            var currentTime = DateTime.Now;

            var mods = CalculatorUtils.GetMods(ruleset, args);
            var scoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = 1,
                MaxCombo = CalculatorUtils.GetMaxCombo(beatmap, mode),
                Statistics = CalculatorUtils.GenerateHitResultsForSs(beatmap, mode),
                Mods = mods
            };

            var performanceCalculator = ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);

            var elapsed = DateTime.Now - currentTime;
            LogUtils.DebugLogger("Calculated Map PerformanceAttributes! (Total Time: " + elapsed.Milliseconds + " milliseconds)");

            return new MapPerformanceAttributes
            {
                Mods = args.Mods,
                PerformanceAttributes = performanceAttributes
            };
        }

        // Copyright(c) 2019 ppy Pty Ltd <contact@ppy.sh>.
        // This code is borrowed from osu-tools(https://github.com/ppy/osu-tools)
        // osu-tools is licensed under the MIT License. https://github.com/ppy/osu-tools/blob/master/LICENCE
        internal StrainList GetStrainLists()
        {
            try
            {
                var difficultyCalculator = CalculatorUtils.GetExtendedDifficultyCalculator(ruleset.RulesetInfo, workingBeatmap);
                difficultyCalculator.Calculate();

                if (difficultyCalculator is IExtendedDifficultyCalculator extendedDifficultyCalculator)
                {
                    var skills = extendedDifficultyCalculator.GetSkills();

                    List<float[]> strainLists = [];

                    foreach (var skill in skills)
                    {
                        double[] strains = [.. ((StrainSkill)skill).GetCurrentStrainPeaks()];

                        var skillStrainList = new List<float>();

                        for (int i = 0; i < strains.Length; i++)
                        {
                            double strain = strains[i];
                            skillStrainList.Add((float)strain);
                        }

                        strainLists.Add([.. skillStrainList]);
                    }

                    return new StrainList
                    {
                        Strains = strainLists,
                        SkillNames = [.. skills.Select(skill => skill.GetType().Name)]
                    };
                }
                else
                {
                    return new StrainList();
                }
            }
            catch (Exception e)
            {
                LogUtils.DebugLogger("Error getting strain lists: " + e.Message, true);
                return new StrainList();
            }
        }

        internal int GetFirstObjectTime()
        {
            var firstObject = workingBeatmap.Beatmap.HitObjects.Count > 1 ? workingBeatmap.Beatmap.HitObjects[1] : null;
            return (int)(firstObject?.StartTime ?? 0);
        }
    }
}
