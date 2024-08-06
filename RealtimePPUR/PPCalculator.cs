using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using System.Linq;
using RealtimePPUR.Classes;
using static RealtimePPUR.Classes.CalculatorHelpers;

namespace RealtimePPUR
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
            difficultyAttributes.MaxCombo = GetMaxCombo(beatmap, mode);

            var performanceCalculator = ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);

            data.DifficultyAttributes = difficultyAttributes;
            data.PerformanceAttributes = performanceAttributes;
            data.CurrentDifficultyAttributes = difficultyAttributes;
            data.CurrentPerformanceAttributes = performanceAttributes;
            data.DifficultyAttributesIffc = difficultyAttributes;
            data.PerformanceAttributesIffc = performanceAttributes;
            data.IfFcHitResult = staticsSs;
            data.ExpectedManiaScore = 0;

            var statisticsCurrent = GenerateHitResultsForCurrent(hits, mode);

            if (resultScreen)
            {
                var resultScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                {
                    Accuracy = args.Accuracy / 100,
                    MaxCombo = args.Combo,
                    Statistics = statisticsCurrent,
                    Mods = mods
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
                if (mode != 3)
                {
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
                }
                else
                {
                    data.ExpectedManiaScore = ManiaScoreCalculator(beatmap, hits, args.Mods, args.Score);
                }

                if (args.PplossMode && mode is 1 or 3)
                {
                    var staticsLoss = GenerateHitResultsForLossMode(beatmap, hits, mode);

                    var lossScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
                    {
                        Accuracy = GetAccuracy(staticsLoss, mode),
                        MaxCombo = args.Combo,
                        Statistics = staticsLoss,
                        Mods = mods,
                        TotalScore = args.Score
                    };

                    var performanceAttributesCurrent = performanceCalculator?.Calculate(lossScoreInfo, difficultyAttributes);

                    data.CurrentDifficultyAttributes = difficultyAttributes;
                    data.CurrentPerformanceAttributes = performanceAttributesCurrent;
                }
                else
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
                    var performanceCalculatorCurrent = ruleset.CreatePerformanceCalculator();
                    var performanceAttributesCurrent = performanceCalculatorCurrent?.Calculate(currentScoreInfo, difficultyAttributesCurrent);

                    data.CurrentDifficultyAttributes = difficultyAttributesCurrent;
                    data.CurrentPerformanceAttributes = performanceAttributesCurrent;


                    // おふざけ機能

                    // 現在のBPM (IfFCのところに表示)
                    //var currentBpm = beatmapCurrent.ControlPointInfo.TimingPoints.OfType<TimingControlPoint>().Last(tp => tp.Time <= args.Time).BPM;
                    //data.PerformanceAttributesIffc.Total = currentBpm;

                    // 現在のSV (CurrentPPのところに表示)
                    //var currentSv = beatmapCurrent.ControlPointInfo.AllControlPoints.OfType<DifficultyControlPoint>().Last(tp => tp.Time <= args.Time).SliderVelocity;
                    //data.CurrentPerformanceAttributes.Total = currentSv;

                    // おふざけ機能
                }

                return data;
            }
        }
    }
}
