using RealtimePPUR.Services.PPCalculation;

namespace RealtimePPUR.Models;

public class OsuBetmapInfo
{
    public ProcessorWorkingBeatmap? ProcessorWorkingBeatmap { get; set; } = null;
    public OsuGameMode BeatmapGameMode { get; set; } = OsuGameMode.None;
}
