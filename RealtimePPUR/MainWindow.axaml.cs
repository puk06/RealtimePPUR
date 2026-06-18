using Avalonia.Controls;
using RealtimePPUR.Services;
using Avalonia.Threading;
using System;

namespace RealtimePPUR;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RealtimePPCalculator.Instance.Start();
        RealtimePPCalculator.Instance.OnCalculate += OnUpdate;
    }

    private async void OnUpdate()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var attributes = RealtimePPCalculator.Instance.CurrentAttributes;
            var memoryData = RealtimePPCalculator.Instance.CurrentMemoryData;
            SrValue.Text = attributes.CurrentStarRating.ToString("F2");

            string ifFcString = string.Empty;
            var lossModePp = Math.Round(attributes.LossModePerformancePoint).ToString("F0");
            var iffcPp = Math.Round(attributes.IfFCPerformancePoint).ToString("F0");
            var ssPp = Math.Round(attributes.MapPerformanceAttributes?.Total ?? 0).ToString("F0");
            if (memoryData.IsPlaying)
            {
                var isLossModeAvailable = memoryData.CurrentGameMode == Models.OsuGameMode.Taiko || memoryData.CurrentGameMode == Models.OsuGameMode.Mania;
                if (isLossModeAvailable) ifFcString = lossModePp + " / " + ssPp;
                else ifFcString = iffcPp + " / " + ssPp;
            }
            else if (memoryData.IsResultScreen)
            {
                ifFcString = iffcPp + " / " + ssPp;
            }
            else
            {
                ifFcString = ssPp;
            }

            IffcValue.Text = ifFcString;
            PpValue.Text = attributes.CurrentPerformancePoint.ToString("F2");

            OffsetValue.Text = Math.Round(attributes.HitErrorInfo.Average).ToString("F0");
            AvgValue.Text = (-attributes.HitErrorInfo.Average).ToString("F2");

            UrValue.Text = attributes.HitErrorInfo.UnstableRate.ToString("F2");
            
            try
            {
                Count300.Text = attributes.CurrentHitResults[osu.Game.Rulesets.Scoring.HitResult.Great].ToString();
                Count100.Text = attributes.CurrentHitResults[osu.Game.Rulesets.Scoring.HitResult.Ok].ToString();
                Count50.Text = attributes.CurrentHitResults[osu.Game.Rulesets.Scoring.HitResult.Miss].ToString();
            }
            catch
            {
                // Ignore
            }
        });
    }
}
