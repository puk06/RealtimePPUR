using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using OsuMemoryDataProvider;
using RealtimePPUR.Data;
using RealtimePPUR.Models;
using RealtimePPUR.Services.PPCalculation;
using RealtimePPUR.Utils;

namespace RealtimePPUR.Services;

public class RealtimePPCalculator
{
    public event Action? OnCalculate = null;
    private MemoryReader MemoryReader { get; } = new();
    private SimplifiedAttributes SimplifiedAttributes { get; } = new();
    private CachedMemoryData CachedMemoryData { get; } = new();
    private static PerformanceCalculationContext Args { get; } = new();

    public static RealtimePPCalculator Instance => _instance;
    private static readonly RealtimePPCalculator _instance = new();

    public MemoryData CurrentMemoryData => MemoryReader.CurrentMemoryData;
    public SimplifiedAttributes CurrentAttributes => SimplifiedAttributes;

    public OsuBetmapInfo OsuBeatmapInfo { get; } = new();
    public OsuGameMode CurrentCalculationGameMode => CachedMemoryData.PreviousCalculatedGameMode;

    private readonly SettingsManager<RuntimeSettings> settingsManager = new(SystemPath.RuntimeSettingsFilePath);
    public RuntimeSettings RuntimeSettings => settingsManager.Settings;

    private RealtimePPCalculator()
    {
    }

    public void Start()
    {
        MemoryReader.Start();
        settingsManager.Load();
        new Thread(Update) { IsBackground = true }.Start();
    }

    public void UpdateSettings(RuntimeSettings runtimeSettings)
    {
        settingsManager.Update(runtimeSettings);
    }

    public void SaveSettings()
    {
        settingsManager.Save();
    }


    private async void Update()
    {
        while (true)
        {
            await CalculatePerformance();
            Thread.Sleep(Math.Max(RuntimeSettings.CalculationInterval, 0));
        }
    }

    private async Task CalculatePerformance()
    {
        try
        {
            bool recalculateRequired = false;
            bool recalculateMapAttributesRequired = false;

            var cached = CachedMemoryData;
            var current = MemoryReader.CurrentMemoryData;

            string previousSongsPath = cached.PreviousSongsPath;
            string currentSongsPath = string.IsNullOrEmpty(RuntimeSettings.CustomSongsFolder) ? current.OsuPathInfo.SongsPath : RuntimeSettings.CustomSongsFolder;
            var songsFolderHasChanged = previousSongsPath != currentSongsPath;
            cached.PreviousSongsPath = currentSongsPath;

            string previousMapPath = cached.PreviousMapPath;
            string currentMapPath = current.OsuMapInfo.RelativeBeatmapPath;
            if (songsFolderHasChanged || previousMapPath != currentMapPath)
            {
                var fixedBeatmapPath = OsuBeatmapUtils.GetMapPath(
                    currentSongsPath,
                    current.OsuMapInfo.FolderName,
                    current.OsuMapInfo.FileName
                );

                OsuBeatmapInfo.BeatmapGameMode = OsuGameMode.None;

                if (!string.IsNullOrEmpty(fixedBeatmapPath))
                {
                    OsuBeatmapInfo.ProcessorWorkingBeatmap = ProcessorWorkingBeatmap.FromFile(fixedBeatmapPath);
                    OsuBeatmapInfo.BeatmapGameMode = await OsuBeatmapUtils.GetMapMode(fixedBeatmapPath);
                    recalculateRequired = true;
                    recalculateMapAttributesRequired = true;
                }

                cached.PreviousMapPath = currentMapPath;
            }

            var previousCalculatedGameMode = cached.PreviousCalculatedGameMode;
            var currentGameMode = GetCalculationMode(current.OsuGameMode, OsuBeatmapInfo.BeatmapGameMode, current.OsuMemoryStatus);
            if (previousCalculatedGameMode != currentGameMode)
            {
                recalculateRequired = true;
                recalculateMapAttributesRequired = true;
                cached.PreviousCalculatedGameMode = currentGameMode;
            }

            var previousHitResult = cached.PreviousHitResult;
            var currentHitResult = current.HitResult;
            if (!currentHitResult.Equals(previousHitResult))
            {
                recalculateRequired = true;
                cached.PreviousHitResult.FromOther(current.HitResult);
            }

            var previousScore = cached.PreviousScore;
            var currentScore = current.CurrentScore;
            if (previousScore != currentScore)
            {
                recalculateRequired = true;
                cached.PreviousScore = current.CurrentScore;
            }

            var previousCombo = cached.PreviousCombo;
            var currentCombo = current.CurrentCombo;
            if (previousCombo != currentCombo)
            {
                recalculateRequired = true;
                cached.PreviousCombo = current.CurrentCombo;
            }

            var previousOsuStatus = cached.PreviousOsuMemoryStatus;
            var currentOsuStatus = current.OsuMemoryStatus;
            if (previousOsuStatus != currentOsuStatus)
            {
                recalculateRequired = true;
                recalculateMapAttributesRequired = true;
                cached.PreviousOsuMemoryStatus = currentOsuStatus;
            }

            var previousMods = cached.PreviousMods;
            var currentMods = current.CurrentMods;
            if (previousMods != currentMods)
            {
                recalculateRequired = true;
                recalculateMapAttributesRequired = true;
                cached.PreviousMods = current.CurrentMods;
            }

            bool updated = false;

            if (recalculateRequired || recalculateMapAttributesRequired)
            {
                Args.GameMode = currentGameMode;
                Args.IsResultScreen = current.IsResultScreen;
                Args.IsPlaying = current.IsPlaying;
                Args.Score = current.CurrentScore;
                Args.AudioTime = current.CurrentAudioTime;
                Args.Mods = OsuModParser.ToOsuMods(currentGameMode, current.CurrentMods);

                if (recalculateMapAttributesRequired) PPCalculator.CalculateMapAttributes(Args, OsuBeatmapInfo, SimplifiedAttributes);
                PPCalculator.Calculate(Args, OsuBeatmapInfo, SimplifiedAttributes, current.HitResult);

                updated = true;
            }

            if (current.IsPlaying)
            {
                lock (MemoryReader.LockObject)
                {
                    if (current.HitErrors != null)
                    {
                        var previousHitErrorsCount = cached.PreviousHitErrorsCount;
                        var currentHitErrorsCount = current.HitErrors.Count;

                        if (previousHitErrorsCount != currentHitErrorsCount)
                        {
                            SimplifiedAttributes.HitErrorInfo.UnstableRate = CalculateUnstableRate(current.HitErrors);
                            SimplifiedAttributes.HitErrorInfo.Average = CalculateAverage(current.HitErrors);
                            cached.PreviousHitErrorsCount = currentHitErrorsCount;

                            updated = true;
                        }
                    }
                }
            }

            if (updated) OnCalculate?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private static double CalculateUnstableRate(List<int> hitErrors)
    {
        if (hitErrors.Count == 0) return 0;

        double totalAll = hitErrors.Sum(hit => (long)hit);
        double average = totalAll / hitErrors.Count;
        double variance = hitErrors.Sum(hit => Math.Pow(hit - average, 2)) / hitErrors.Count;
        double unstableRate = Math.Sqrt(variance) * 10;

        return unstableRate;
    }

    private static double CalculateAverage(List<int> hitErrors)
    {
        if (hitErrors.Count == 0) return 0;

        var sortedArray = hitErrors.OrderBy(x => x).ToArray();

        var count = sortedArray.Length;
        double q1 = sortedArray[(int)(count * 0.25)];
        double q3 = sortedArray[(int)(count * 0.75)];
        double iqr = q3 - q1;

        var avg = sortedArray.Where(x => x >= q1 - (1.5 * iqr) && x <= q3 + (1.5 * iqr)).Average();

        return avg;
    }

    private static OsuGameMode GetCalculationMode(OsuGameMode osu, OsuGameMode beatmap, OsuMemoryStatus status)
    {
        if (beatmap == OsuGameMode.None) return osu;
        else if (status == OsuMemoryStatus.EditingMap || beatmap != OsuGameMode.Osu) return beatmap;
        else return osu;
    }
}

public class PerformanceCalculationContext
{
    public OsuGameMode GameMode { get; set; } = OsuGameMode.None;
    public bool IsResultScreen { get; set; } = false;
    public bool IsPlaying { get; set; } = false;
    public int Score { get; set; } = 0;
    public int Combo { get; set; } = 0;
    public int AudioTime { get; set; } = 0;
    public Mod[] Mods { get; set; } = [];
}

public class SimplifiedAttributes
{
    public DifficultyAttributes? MapDifficultyAttributes { get; set; }
    public PerformanceAttributes? MapPerformanceAttributes { get; set; }

    public double CurrentStarRating { get; set; } = 0;
    public double CurrentPerformancePoint { get; set; } = 0;
    public Dictionary<osu.Game.Rulesets.Scoring.HitResult, int> CurrentHitResults = new();
    public double CurrentAccuracy { get; set; } = 0;

    public double IfFCPerformancePoint { get; set; } = 0;
    public Dictionary<osu.Game.Rulesets.Scoring.HitResult, int> IfFCHitResults = new();
    public double IfFCAccuracy { get; set; } = 0;

    public double LossModePerformancePoint { get; set; } = 0;
    public Dictionary<osu.Game.Rulesets.Scoring.HitResult, int> LossModeHitResults = new();
    public double LossModeAccuracy { get; set; } = 0;

    public HitErrorInfo HitErrorInfo { get; } = new();
    public int TotalHitObjectsCount { get; set; } = 0;
}

public class CachedMemoryData
{
    public HitResult PreviousHitResult { get; } = new();
    public int PreviousScore { get; set; } = 0;
    public int PreviousCombo { get; set; } = 0;
    public int PreviousMods { get; set; } = 0;
    public string PreviousSongsPath { get; set; } = string.Empty;
    public string PreviousMapPath { get; set; } = string.Empty;
    public OsuMemoryStatus PreviousOsuMemoryStatus { get; set; } = OsuMemoryStatus.Unknown;
    public OsuGameMode PreviousCalculatedGameMode { get; set; } = OsuGameMode.None;
    public int PreviousHitErrorsCount { get; set; } = 0;
}
