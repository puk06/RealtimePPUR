using System;
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
        private MapPerformanceAttributes currentMapPerformanceAttributes;
        private int totalHitObjectCount;

        public void SetMap(string file, int givenmode)
        {
            mode = givenmode;
            ruleset = SetRuleset(givenmode);
            workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);
            currentDifficultyAttributes = null;
            currentMapDifficultyAttributes = null;
            currentMapPerformanceAttributes = null;
        }

        public void SetMode(int givenmode)
        {
            ruleset = SetRuleset(givenmode);
            mode = givenmode;
            currentDifficultyAttributes = null;
            currentMapDifficultyAttributes = null;
            currentMapPerformanceAttributes = null;
        }

        public BeatmapData Calculate(CalculateArgs args, bool playing, bool resultScreen, HitsResult hits)
        {
            var mods = GetMods(ruleset, args);
            var beatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);

            var staticsSs = GenerateHitResultsForSs(beatmap, mode);

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
                var performanceCalculator = ruleset.CreatePerformanceCalculator();
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
                data.IfFcHitResult = staticsForCalcIfFc;

                return data;
            }

            if (!playing) return data;
            {
                var performanceCalculator = ruleset.CreatePerformanceCalculator();
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

                // Calculate Loss Mode PP
                if (mode is 1 or 3)
                {
                    var staticsLoss = GenerateHitResultsForLossMode(staticsSs, hits, mode);
                    data.HitResultLossMode = staticsLoss;

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

                // Calculate IFFC
                if (mode is not 3)
                {
                    var staticsForCalcIfFc = CalcIfFc(beatmap, hits, mode);
                    data.IfFcHitResult = staticsForCalcIfFc;

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

                // Get Current BPM
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
                data.CurrentBpm = lastTimingPoint.BPM;

                return data;
            }
        }

        private DifficultyAttributes GetCurrentDifficultyAttributes(IBeatmap beatmap, CalculateArgs args, int? time)
        {
            time ??= 0;
            currentDifficultyAttributes ??= CalculateAllTimedDifficulties(beatmap, args);
            var difficultyAttributes = currentDifficultyAttributes.LastOrDefault(d => d.Time <= time);
            if (difficultyAttributes != null) return difficultyAttributes.DifficultyAttributes;
            difficultyAttributes = currentDifficultyAttributes.First();
            return difficultyAttributes.DifficultyAttributes;
        }

        private DifficultyAttributes GetCurrentMapDifficultyAttributes(CalculateArgs args, IBeatmap beatmap)
        {
            if (currentMapDifficultyAttributes != null && !currentMapDifficultyAttributes.Mods.SequenceEqual(args.Mods))
            {
                DebugLogger("Mods changed, recalculating Map DifficultyAttributes...");
                currentMapDifficultyAttributes = null;
            }

            currentMapDifficultyAttributes ??= CalculateMapDifficultyAttributes(args, beatmap);
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

            var total = hitObjects.Length;
            DebugLogger($"Calculating All DifficultyAttributes... Total: {total}");
            var currentTime = DateTime.Now;

            var currentProgress = 0;
            foreach (var hitObject in hitObjects)
            {
                var progress = (int)(hitObject.StartTime / hitObjects[^1].StartTime * 100);
                if (progress != currentProgress)
                {
                    currentProgress = progress;
                    Console.Write($"\rCalculating Progress: {progress}%");
                }

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

            var elapsed = DateTime.Now - currentTime;
            Console.WriteLine();
            DebugLogger($"Calculated All DifficultyAttributes! (Total Time: {elapsed.TotalSeconds} seconds)");

            return allTimedDifficulties;
        }

        private MapDifficultyAttributes CalculateMapDifficultyAttributes(CalculateArgs args, IBeatmap beatmap)
        {
            DebugLogger("Calculating Map DifficultyAttributes...");
            var currentTime = DateTime.Now;

            var mods = GetMods(ruleset, args);
            var difficultyCalculator = ruleset.CreateDifficultyCalculator(workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);

            var elapsed = DateTime.Now - currentTime;
            DebugLogger("Calculated Map DifficultyAttributes! (Total Time: " + elapsed.Milliseconds + " milliseconds)");

            totalHitObjectCount = CountTotalHitObjects(beatmap, mode);
            DebugLogger("Total HitObject Count: " + totalHitObjectCount);

            return new MapDifficultyAttributes
            {
                Mods = args.Mods,
                DifficultyAttributes = difficultyAttributes
            };
        }

        private MapPerformanceAttributes GetCurrentMapPerformanceAttributes(CalculateArgs args, IBeatmap beatmap,
            DifficultyAttributes difficultyAttributes)
        {
            if (currentMapPerformanceAttributes != null &&
                !currentMapPerformanceAttributes.Mods.SequenceEqual(args.Mods))
            {
                DebugLogger("Mods changed, recalculating Map PerformanceAttributes...");
                currentMapPerformanceAttributes = null;
            }

            currentMapPerformanceAttributes ??= CalculateMapPerformanceAttributes(args, beatmap, difficultyAttributes);
            return currentMapPerformanceAttributes;
        }

        private MapPerformanceAttributes CalculateMapPerformanceAttributes(CalculateArgs args, IBeatmap beatmap,
            DifficultyAttributes difficultyAttributes)
        {
            DebugLogger("Calculating Map PerformanceAttributes...");
            var currentTime = DateTime.Now;

            var mods = GetMods(ruleset, args);
            var scoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = 1,
                MaxCombo = GetMaxCombo(beatmap, mode),
                Statistics = GenerateHitResultsForSs(beatmap, mode),
                Mods = mods
            };

            var performanceCalculator = ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);

            var elapsed = DateTime.Now - currentTime;
            DebugLogger("Calculated Map PerformanceAttributes! (Total Time: " + elapsed.Milliseconds +
                        " milliseconds)");

            return new MapPerformanceAttributes
            {
                Mods = args.Mods,
                PerformanceAttributes = performanceAttributes
            };
        }
    }
}
