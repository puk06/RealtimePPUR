using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using static RealtimePPUR.Classes.CalculatorHelpers;
using static RealtimePPUR.Classes.Helper;

namespace RealtimePPUR.Classes
{
    public class PpCalculator(string file, int mode)
    {
        private Ruleset ruleset = SetRuleset(mode);
        private ProcessorWorkingBeatmap workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);
        private List<TimedDifficultyAttributes> currentDifficultyAttributes;
        private MapDifficultyAttributes currentMapDifficultyAttributes;

        public void SetMap(string file, int givenmode)
        {
            mode = givenmode;
            ruleset = SetRuleset(givenmode);
            workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);
            currentDifficultyAttributes = null;
            currentMapDifficultyAttributes = null;
        }

        public void SetMode(int givenmode)
        {
            ruleset = SetRuleset(givenmode);
            mode = givenmode;
            currentDifficultyAttributes = null;
            currentMapDifficultyAttributes = null;
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

            //var difficultyCalculator = ruleset.CreateDifficultyCalculator(workingBeatmap);
            //var difficultyAttributes = difficultyCalculator.Calculate(mods);
            var difficultyAttributes = GetCurrentMapDifficultyAttributes(args);

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
                var performanceAttributesResult =
                    performanceCalculator?.Calculate(resultScoreInfo, difficultyAttributes);
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
                DifficultyAttributes difficultyAttributesCurrent;

                if (args.CalculateBeforePlaying)
                {
                    difficultyAttributesCurrent = GetCurrentDifficultyAttributes(beatmap, args, args.Time);
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
                    Accuracy = GetAccuracy(statisticsCurrent, mode),
                    MaxCombo = args.Combo,
                    Statistics = statisticsCurrent,
                    Mods = mods,
                    TotalScore = args.Score
                };

                var performanceCalculatorCurrent = ruleset.CreatePerformanceCalculator();
                var performanceAttributesCurrent =
                    performanceCalculatorCurrent?.Calculate(currentScoreInfo, difficultyAttributesCurrent);

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

                    var performanceAttributesLossMode =
                        performanceCalculator?.Calculate(lossScoreInfo, difficultyAttributes);
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

                    var performanceAttributesIffc =
                        performanceCalculator?.Calculate(iffcScoreInfo, difficultyAttributes);
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

        private DifficultyAttributes GetCurrentDifficultyAttributes(IBeatmap beatmap, CalculateArgs args, int? time)
        {
            time ??= 0;
            currentDifficultyAttributes ??= CalculateAllTimedDifficulties(beatmap, args);
            var difficultyAttributes = currentDifficultyAttributes.LastOrDefault(d => d.Time <= time);
            return difficultyAttributes?.DifficultyAttributes;
        }

        private DifficultyAttributes GetCurrentMapDifficultyAttributes(CalculateArgs args)
        {
            if (currentMapDifficultyAttributes != null && !currentMapDifficultyAttributes.Mods.SequenceEqual(args.Mods))
            {
                DebugLogger("Mods changed, recalculating Map DifficultyAttributes...");
                currentMapDifficultyAttributes = null;
            }
            currentMapDifficultyAttributes ??= CalculateMapDifficultyAttributes(args);
            return currentMapDifficultyAttributes.DifficultyAttributes;
        }

        private List<TimedDifficultyAttributes> CalculateAllTimedDifficulties(IBeatmap beatmap, CalculateArgs args)
        {
            var allTimedDifficulties = new List<TimedDifficultyAttributes>();
            var hitObjects = beatmap.HitObjects.ToArray();
            Beatmap beatmapCurrent = new()
            {
                ControlPointInfo = workingBeatmap.Beatmap.ControlPointInfo,
                BeatmapInfo = workingBeatmap.Beatmap.BeatmapInfo
            };

            var mods = GetMods(ruleset, args);

            DebugLogger("Calculating All DifficultyAttributes...");
            foreach (var hitObject in hitObjects)
            {
                var o = hitObject;
                var hitObjectsCurrent = hitObjects.Where(h => h.StartTime <= o.StartTime).ToList();
                beatmapCurrent.HitObjects.Clear();
                beatmapCurrent.HitObjects.AddRange(hitObjectsCurrent);

                var difficultyCalculator =
                    ruleset.CreateDifficultyCalculator(new ProcessorWorkingBeatmap(beatmapCurrent));
                var difficultyAttributes = difficultyCalculator.Calculate(mods);
                allTimedDifficulties.Add(new TimedDifficultyAttributes
                {
                    Time = hitObject.StartTime,
                    DifficultyAttributes = difficultyAttributes
                });
            }

            DebugLogger("Calculated All DifficultyAttributes!");

            return allTimedDifficulties;
        }

        private MapDifficultyAttributes CalculateMapDifficultyAttributes(CalculateArgs args)
        {
            DebugLogger("Calculating Map DifficultyAttributes...");
            var mods = GetMods(ruleset, args);
            var difficultyCalculator = ruleset.CreateDifficultyCalculator(workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);
            DebugLogger("Calculated Map DifficultyAttributes!");
            return new MapDifficultyAttributes
            {
                Mods = args.Mods,
                DifficultyAttributes = difficultyAttributes
            };
        }
    }
}
