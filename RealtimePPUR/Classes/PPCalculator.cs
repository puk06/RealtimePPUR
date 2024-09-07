using System;
using System.Diagnostics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using System.Linq;
using static RealtimePPUR.Classes.CalculatorHelpers;

namespace RealtimePPUR.Classes
{
    public class PpCalculator(string file, int mode)
    {
        private Ruleset ruleset = SetRuleset(mode);
        private ProcessorWorkingBeatmap workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);

        public void SetMap(string file, int givenmode)
        {
            mode = givenmode;
            ruleset = SetRuleset(givenmode);
            workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);
        }

        public void SetMode(int givenmode)
        {
            ruleset = SetRuleset(givenmode);
            mode = givenmode;
        }

        public BeatmapData Calculate(CalculateArgs args, bool playing, bool resultScreen, HitsResult hits)
        {
            var data = new BeatmapData();
            var mods = GetMods(ruleset, args);
            var beatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);

            var staticsSs = GenerateHitResultsForSs(beatmap, mode);

            var scoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = 1,
                MaxCombo = GetMaxCombo(beatmap, mode),
                Statistics = staticsSs,
                Mods = mods
            };

            var difficultyCalculator = ruleset.CreateDifficultyCalculator(workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);

            // Fix the combo for osu! standard
            var maxCombo = GetMaxCombo(beatmap, mode);
            difficultyAttributes.MaxCombo = maxCombo;

            var performanceCalculator = ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);

            data.DifficultyAttributes = difficultyAttributes;
            data.PerformanceAttributes = performanceAttributes;
            data.CurrentDifficultyAttributes = difficultyAttributes;
            data.CurrentPerformanceAttributes = performanceAttributes;
            data.DifficultyAttributesIffc = difficultyAttributes;
            data.PerformanceAttributesIffc = performanceAttributes;
            data.PerformanceAttributesLossMode = performanceAttributes;
            data.IfFcHitResult = staticsSs;
            data.ExpectedManiaScore = 0;
            data.CurrentBpm = 0;

            var statisticsCurrent = GenerateHitResultsForCurrent(hits, mode);
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
                var performanceAttributesResult = performanceCalculator?.Calculate(resultScoreInfo, difficultyAttributes);
                data.CurrentPerformanceAttributes = performanceAttributesResult;

                if (mode == 3) return data;
                var staticsForCalcIfFc = CalcIfFc(beatmap, hits, mode);
                var iffcScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = GetAccuracy(staticsForCalcIfFc, mode),
                    MaxCombo = GetMaxCombo(beatmap, mode),
                    Statistics = staticsForCalcIfFc,
                    Mods = mods,
                    TotalScore = args.Score
                };
                var performanceAttributesIffc = performanceCalculator?.Calculate(iffcScoreInfo, difficultyAttributes);
                data.PerformanceAttributesIffc = performanceAttributesIffc;

                return data;
            }

            if (!playing) return data;
            {
                Beatmap beatmapCurrent = new();
                var hitObjects = workingBeatmap.Beatmap.HitObjects.Where(h => h.StartTime <= args.Time).ToList();
                beatmapCurrent.HitObjects.AddRange(hitObjects);
                beatmapCurrent.ControlPointInfo = workingBeatmap.Beatmap.ControlPointInfo;
                beatmapCurrent.BeatmapInfo = workingBeatmap.Beatmap.BeatmapInfo;
                var currentScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = GetAccuracy(statisticsCurrent, mode),
                    MaxCombo = args.Combo,
                    Statistics = statisticsCurrent,
                    Mods = mods,
                    TotalScore = args.Score
                };

                var workingBeatmapCurrent = new ProcessorWorkingBeatmap(beatmapCurrent);
                var difficultyCalculatorCurrent = ruleset.CreateDifficultyCalculator(workingBeatmapCurrent);
                var difficultyAttributesCurrent = difficultyCalculatorCurrent.Calculate(mods);

                // Fix the combo for osu! standard
                difficultyAttributesCurrent.MaxCombo = maxCombo;

                var performanceCalculatorCurrent = ruleset.CreatePerformanceCalculator();
                var performanceAttributesCurrent = performanceCalculatorCurrent?.Calculate(currentScoreInfo, difficultyAttributesCurrent);

                data.CurrentDifficultyAttributes = difficultyAttributesCurrent;
                data.CurrentPerformanceAttributes = performanceAttributesCurrent;

                var staticsLoss = GenerateHitResultsForLossMode(staticsSs, hits, mode);
                var staticsForCalcIfFc = CalcIfFc(beatmap, hits, mode);

                data.HitResultLossMode = staticsLoss;
                data.IfFcHitResult = staticsForCalcIfFc;

                if (mode is 1 or 3)
                {
                    var lossScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                    {
                        Accuracy = GetAccuracy(staticsLoss, mode),
                        MaxCombo = args.Combo,
                        Statistics = staticsLoss,
                        Mods = mods,
                        TotalScore = args.Score
                    };

                    var performanceAttributesLossMode = performanceCalculator?.Calculate(lossScoreInfo, difficultyAttributes);
                    data.PerformanceAttributesLossMode = performanceAttributesLossMode;
                    if (mode == 3) data.ExpectedManiaScore = ManiaScoreCalculator(beatmap, hits, args.Mods, args.Score);
                }

                if (mode is not 3)
                {
                    var iffcScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                    {
                        Accuracy = GetAccuracy(staticsForCalcIfFc, mode),
                        MaxCombo = GetMaxCombo(beatmap, mode),
                        Statistics = staticsForCalcIfFc,
                        Mods = mods,
                        TotalScore = args.Score
                    };

                    var performanceAttributesIffc = performanceCalculator?.Calculate(iffcScoreInfo, difficultyAttributes);
                    data.PerformanceAttributesIffc = performanceAttributesIffc;
                }

                var timingPoints = beatmap.ControlPointInfo.TimingPoints;
                TimingControlPoint lastTimingPoint = null;

                foreach (var tp in timingPoints)
                {
                    if (tp is not null && tp.Time <= args.Time)
                    {
                        lastTimingPoint = tp;
                    }
                }

                if (lastTimingPoint == null) return data;

                var currentBpm = lastTimingPoint.BPM;
                data.CurrentBpm = currentBpm;

                return data;
            }
        }
    }
}
