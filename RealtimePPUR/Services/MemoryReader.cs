using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using RealtimePPUR.Models;

namespace RealtimePPUR.Services;

public class MemoryReader
{
    private const string processName = "osu!";
    private readonly StructuredOsuMemoryReader _memoryReader = StructuredOsuMemoryReader.GetInstance(new(processName));
    private readonly OsuBaseAddresses baseAddresses = new();

    private readonly MemoryData _memoryData = new();

    private readonly int retrieveInterval = 15;
    private readonly int processRetrieveInterval = 1000;
    public static readonly Lock LockObject = new();

    public MemoryData CurrentMemoryData => _memoryData;

    public void Start()
    {
        new Thread(UpdateMemory) { IsBackground = true }.Start();
        new Thread(ProcessWatcher) { IsBackground = true }.Start();
    }

    private void UpdateMemory()
    {
        while (true)
        {
            try
            {
                if (!_memoryReader.CanRead)
                {
                    Debug.WriteLine("Memory couldn't read.");
                    continue;
                }

                lock (LockObject)
                {
                    _memoryReader.TryRead(baseAddresses.Beatmap);
                    _memoryReader.TryRead(baseAddresses.Player);
                    _memoryReader.TryRead(baseAddresses.GeneralData);
                    _memoryReader.TryRead(baseAddresses.LeaderBoard);
                    _memoryReader.TryRead(baseAddresses.ResultsScreen);
                    _memoryReader.TryRead(baseAddresses.BanchoUser);
                
                    _memoryData.OsuMemoryStatus = baseAddresses.GeneralData.OsuStatus;
                    _memoryData.OsuGameMode = _memoryData.OsuMemoryStatus switch
                    {
                        OsuMemoryStatus.Playing => (OsuGameMode)baseAddresses.Player.Mode,
                        OsuMemoryStatus.ResultsScreen => (OsuGameMode)baseAddresses.ResultsScreen.Mode,
                        _ => (OsuGameMode)baseAddresses.GeneralData.GameMode
                    };

                    _memoryData.OsuMapInfo.FolderName = baseAddresses.Beatmap.FolderName;
                    _memoryData.OsuMapInfo.FileName = baseAddresses.Beatmap.OsuFileName;

                    _memoryData.CurrentAudioTime = baseAddresses.GeneralData.AudioTime;

                    _memoryData.CurrentMods = _memoryData.OsuMemoryStatus switch
                    {
                        OsuMemoryStatus.Playing => baseAddresses.Player.Mods.Value,
                        OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Mods.Value,
                        OsuMemoryStatus.MainMenu => baseAddresses.GeneralData.Mods,
                        _ => baseAddresses.GeneralData.Mods
                    };

                    RulesetPlayData? rulesetPlayData = null;
                    if (_memoryData.OsuMemoryStatus == OsuMemoryStatus.Playing || _memoryData.IsPlaying) rulesetPlayData = baseAddresses.Player;
                    else if (_memoryData.OsuMemoryStatus == OsuMemoryStatus.ResultsScreen) rulesetPlayData = baseAddresses.ResultsScreen;

                    _memoryData.HitResult.HitGeki = rulesetPlayData?.HitGeki ?? 0;
                    _memoryData.HitResult.Hit300 = rulesetPlayData?.Hit300 ?? 0;
                    _memoryData.HitResult.HitKatu = rulesetPlayData?.HitKatu ?? 0;
                    _memoryData.HitResult.Hit100 = rulesetPlayData?.Hit100 ?? 0;
                    _memoryData.HitResult.Hit50 = rulesetPlayData?.Hit50 ?? 0;
                    _memoryData.HitResult.HitMiss = rulesetPlayData?.HitMiss ?? 0;
                    _memoryData.CurrentScore = rulesetPlayData?.Score ?? 0; // TODO: アドレスが壊れている
                    _memoryData.CurrentCombo = rulesetPlayData?.Combo ?? 0;
                    _memoryData.CurrentMaxCombo = rulesetPlayData?.MaxCombo ?? 0;

                    _memoryData.HitErrors = baseAddresses.Player.HitErrors;
                    _memoryData.HealthPercentage = baseAddresses.Player.HP;
                }
            }
            catch
            {
                //Ignored
            }
            finally
            {
                Thread.Sleep(retrieveInterval);
            }
        }
    }
    private void ProcessWatcher()
    {
        while (true)
        {
            try
            {
                lock (LockObject)
                {
                    var osuProcesses = ProcessService.GetProcessInfo(processName);

                    _memoryData.OsuProcess = osuProcesses.Process;
                    _memoryData.IsOsuRunning = osuProcesses.IsRunning;
                    _memoryData.OsuPathInfo.OsuProcessDirectory = osuProcesses.Path;
                    _memoryData.OsuPathInfo.SongsPath = Path.Combine(osuProcesses.Path, "Songs");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                Debug.WriteLine("Failed to retrieve processInfo.");
            }
            finally
            {
                Thread.Sleep(processRetrieveInterval);
            }
        }
    }
}

public class MemoryData
{
    public Process? OsuProcess { get; set; } = null;
    public bool IsOsuRunning { get; set; } = false;
    public OsuPathInfo OsuPathInfo { get; } = new();

    public OsuMemoryStatus OsuMemoryStatus { get; set; } = OsuMemoryStatus.Unknown;
    public OsuMapInfo OsuMapInfo { get; } = new();
    public int CurrentAudioTime { get; set; } = 0;

    public OsuGameMode OsuGameMode { get; set; } = OsuGameMode.None;

    public bool IsPlaying => OsuMemoryStatus == OsuMemoryStatus.Playing;
    public bool IsResultScreen => OsuMemoryStatus == OsuMemoryStatus.ResultsScreen;

    public HitResult HitResult { get; } = new();
    public int CurrentCombo { get; set; } = 0;
    public int CurrentMaxCombo { get; set; } = 0;
    public int CurrentScore { get; set; } = 0;
    public int CurrentMods { get; set; } = 0;

    public List<int> HitErrors { get; set; } = new();
    public double HealthPercentage { get; set; } = 0;
}
