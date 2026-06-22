using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RealtimePPUR.Models;
using RealtimePPUR.Services;
using RealtimePPUR.Utils;

namespace RealtimePPUR;

public partial class SettingsWindow : Window
{
    private readonly Dictionary<ToggleSwitch, InGameOverlayValues> InGameOverlaySwitchValues;
    
    public SettingsWindow()
    {
        InitializeComponent();
        InGameOverlaySwitchValues = new()
        {
            { StarRating, InGameOverlayValues.StarRatings },
            { SSPP, InGameOverlayValues.SSPerformancePoint },
            { CurrentPP, InGameOverlayValues.CurrentPerformancePoint },
            { CurrentACC, InGameOverlayValues.CurrentAccuracy },
            { CurrentHits, InGameOverlayValues.CurrentHits },
            { LossModeHits, InGameOverlayValues.LossModeHits },
            { IFFCHits, InGameOverlayValues.IfFCHits },
            { UnstableRate, InGameOverlayValues.UnstableRate },
            { OffsetHelp, InGameOverlayValues.OffsetHelp },
            { AverageError, InGameOverlayValues.AverageError },
            { HealthPercentage, InGameOverlayValues.HealthPercentage },
            { CurrentScore, InGameOverlayValues.Score },
            { CurrentCombo, InGameOverlayValues.Combo },
            { RemainingNotes, InGameOverlayValues.RemainingNotes }
        };
        ResetSettings();

        var platformHandle = TryGetPlatformHandle();
        if (platformHandle != null) ProcessIntPtrManager.Register(typeof(SettingsWindow), platformHandle.Handle);
    }

    private InGameOverlayValues GetCurrentInGameOverlayValues()
    {
        InGameOverlayValues result = 0;

        foreach (var kpv in InGameOverlaySwitchValues)
        {
            var isChecked = kpv.Key?.IsChecked ?? false;
            if (isChecked) result |= kpv.Value;
        }

        return result;
    }
    private void SetFromInGameOverlayValues(InGameOverlayValues inGameOverlayValues)
    {
        foreach (var kpv in InGameOverlaySwitchValues)
        {
            kpv.Key.IsChecked = (inGameOverlayValues & kpv.Value) == kpv.Value;
        }
    }

    public void SetFromSettings(RuntimeSettings runtimeSettings)
    {
        EnableLossMode?.IsChecked = runtimeSettings.EnableLossMode;
        CalculationInterval?.Text = runtimeSettings.CalculationInterval.ToString();
        CustomSongsFolder?.Text = runtimeSettings.CustomSongsFolder;
        EnableOverlay?.IsChecked = runtimeSettings.EnableOverlay;
        OverlayLeft?.Text = runtimeSettings.OverlayLeft.ToString();
        OverlayTop?.Text = runtimeSettings.OverlayTop.ToString();
        AutoCheckUpdateOnStartup?.IsChecked = runtimeSettings.AutoCheckUpdateOnStartup;
        SetFromInGameOverlayValues(runtimeSettings.InGameOverlayValues);
    }

    public void ResetSettings() => SetFromSettings(RealtimePPCalculator.Instance.RuntimeSettings);
    public void ResetToDefaultSettings() => SetFromSettings(RuntimeSettings.Empty);
    public void ApplySettings()
    {
        RealtimePPCalculator.Instance.UpdateSettings(new RuntimeSettings()
        {
            EnableLossMode = EnableLossMode?.IsChecked ?? false,
            CalculationInterval = ValueParser.Int(CalculationInterval?.Text, 15),
            CustomSongsFolder = CustomSongsFolder?.Text ?? string.Empty,
            EnableOverlay = EnableOverlay?.IsChecked ?? false,
            OverlayLeft = ValueParser.Int(OverlayLeft?.Text, 0),
            OverlayTop = ValueParser.Int(OverlayTop?.Text, 75),
            InGameOverlayValues = GetCurrentInGameOverlayValues(),
            AutoCheckUpdateOnStartup = AutoCheckUpdateOnStartup?.IsChecked ?? false,
        });
        RealtimePPCalculator.Instance.SaveSettings();
    }

    public void ResetClicked(object? sender, RoutedEventArgs args) => ResetSettings();
    public void ResetToDefaultClicked(object? sender, RoutedEventArgs args) => ResetToDefaultSettings();
    public void ApplyClicked(object? sender, RoutedEventArgs args) => ApplySettings();
    public void OnClosing(object? sender, WindowClosingEventArgs args)
    {
        args.Cancel = true;
        Hide();
    }
}
