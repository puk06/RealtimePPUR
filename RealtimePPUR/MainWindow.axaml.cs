using System;
using Avalonia.Controls;
using Avalonia.Threading;
using RealtimePPUR.Models;
using RealtimePPUR.Services;

namespace RealtimePPUR;

public partial class MainWindow : Window
{
    private double _displayedPp = 0;
    private double _displayedSr = 0;
    private double _displayedUr = 0;

    private double _targetPp = 0;
    private double _targetSr = 0;
    private double _targetUr = 0;

    private readonly HitResult simplifedHitResult = new();

    private readonly DispatcherTimer _smoothTimer;

    public MainWindow()
    {
        InitializeComponent();
        Topmost = true;

        _smoothTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 60)
        };
        _smoothTimer.Tick += OnSmoothUpdate;
        _smoothTimer.Start();

        RealtimePPCalculator.Instance.Start();
        RealtimePPCalculator.Instance.OnCalculate += OnUpdate;

        new InGameOverlay().Show();
    }

    private DateTime _lastUpdate = DateTime.Now;
    private const double SmoothTime = 0.75;

    private static double Lerp(double current, double target, double t) => current + (target - current) * t;
    private void OnSmoothUpdate(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        // フレームレートに依存しないLerp係数
        var t = 1.0 - Math.Pow(0.01, deltaTime / SmoothTime);

        _displayedPp = Lerp(_displayedPp, _targetPp, t);
        _displayedSr = Lerp(_displayedSr, _targetSr, t);
        _displayedUr = Lerp(_displayedUr, _targetUr, t);

        PpValue.Text = _displayedPp.ToString("F0");
        SrValue.Text = _displayedSr.ToString("F2");
        UrValue.Text = _displayedUr.ToString("F0");
    }

    private async void OnUpdate()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var currentGameMode = RealtimePPCalculator.Instance.CurrentCalculationGameMode;
            var attributes = RealtimePPCalculator.Instance.CurrentAttributes;
            var memoryData = RealtimePPCalculator.Instance.CurrentMemoryData;

            _targetPp = attributes.CurrentPerformancePoint;
            _targetSr = attributes.CurrentStarRating;
            _targetUr = attributes.HitErrorInfo.UnstableRate;

            string ifFcString = string.Empty;
            var lossModePp = Math.Round(attributes.LossModePerformancePoint).ToString("F0");
            var iffcPp = Math.Round(attributes.IfFCPerformancePoint).ToString("F0");
            var ssPp = Math.Round(attributes.MapPerformanceAttributes?.Total ?? 0).ToString("F0");
            if (memoryData.IsPlaying)
            {
                var isLossModeAvailable = currentGameMode == OsuGameMode.Taiko || currentGameMode == OsuGameMode.Mania;
                if (isLossModeAvailable)
                {
                    IffcLabel.Text = "LOSS/SS";
                    ifFcString = lossModePp + " / " + ssPp;
                }
                else
                {
                    IffcLabel.Text = "IFFC/SS";
                    ifFcString = iffcPp + " / " + ssPp;
                }
            }
            else if (memoryData.IsResultScreen)
            {
                IffcLabel.Text = "IFFC/SS";
                ifFcString = iffcPp + " / " + ssPp;
            }
            else
            {
                IffcLabel.Text = "SSPP";
                ifFcString = ssPp;
            }

            IffcValue.Text = ifFcString;

            OffsetValue.Text = Math.Round(attributes.HitErrorInfo.Average).ToString("F0");
            AvgValue.Text = (-attributes.HitErrorInfo.Average).ToString("F2") + "ms";

            SimplifyHits(simplifedHitResult, memoryData.HitResult, currentGameMode);
            Count300.Text = simplifedHitResult.Hit300.ToString();
            Count100.Text = simplifedHitResult.Hit100.ToString();
            CountMiss.Text = simplifedHitResult.HitMiss.ToString();
        });
    }

    private static void SimplifyHits(HitResult target, HitResult original, OsuGameMode mode)
    {
        switch (mode)
        {
            case OsuGameMode.Osu:
                target.Hit300 = original.Hit300;
                target.Hit100 = original.Hit100 + original.Hit50;
                target.HitMiss = original.HitMiss;
                break;
            case OsuGameMode.Taiko:
                target.Hit300 = original.Hit300;
                target.Hit100 = original.Hit100;
                target.HitMiss = original.HitMiss;
                break;
            case OsuGameMode.Catch:
                target.Hit300 = original.Hit300;
                target.Hit100 = original.Hit100 + original.Hit50;
                target.HitMiss = original.HitMiss;
                break;
            case OsuGameMode.Mania:
                target.Hit300 = original.HitGeki + original.Hit300;
                target.Hit100 = original.HitKatu + original.Hit100 + original.Hit50;
                target.HitMiss = original.HitMiss;
                break;
        }
    }
}
