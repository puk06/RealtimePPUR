using osu.Game.IO;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;
using RealtimePPUR.Models;

namespace RealtimePPUR.Utils;

internal class OsuUtils
{
    private static readonly Dictionary<int, string> osu_mods = new()
    {
        { 0, "NM" },
        { 1, "NF" },
        { 2, "EZ" },
        { 4, "TD" },
        { 8, "HD" },
        { 16, "HR" },
        { 32, "SD" },
        { 64, "DT" },
        { 128, "RX" },
        { 256, "HT" },
        { 512, "NC" },
        { 1024, "FL" },
        { 2048, "AT" },
        { 4096, "SO" },
        { 8192, "RX2" },
        { 16384, "PF" },
        { 32768, "4K" },
        { 65536, "5K" },
        { 131072, "6K" },
        { 262144, "7K" },
        { 524288, "8K" },
        { 1048576, "FI" },
        { 2097152, "RD" },
        { 4194304, "CM" },
        { 8388608, "TP" },
        { 16777216, "9K" },
        { 33554432, "CP" },
        { 67108864, "1K" },
        { 134217728, "3K" },
        { 268435456, "2K" },
        { 536870912, "SV2" },
        { 1073741824, "MR" }
    };

    internal static string ConvertHits(int mode, HitsResult hits)
    {
        return mode switch
        {
            0 => $"{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}",
            1 => $"{hits.Hit300}/{hits.Hit100}/{hits.HitMiss}",
            2 => $"{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}",
            3 => $"{hits.HitGeki}/{hits.Hit300}/{hits.HitKatu}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}",
            _ => $"{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}"
        };
    }

    internal static Mods ParseMods(int mods)
    {
        List<string> activeModsCalc = [];
        List<string> activeModsShow = [];

        for (int i = 0; i < 32; i++)
        {
            int bit = 1 << i;
            if ((mods & bit) != bit) continue;
            activeModsCalc.Add(osu_mods[bit].ToLower());
            activeModsShow.Add(osu_mods[bit]);
        }

        if (activeModsCalc.Contains("nc") && activeModsCalc.Contains("dt")) activeModsCalc.Remove("nc");
        if (activeModsShow.Contains("NC") && activeModsShow.Contains("DT")) activeModsShow.Remove("DT");
        if (activeModsShow.Count == 0) activeModsShow.Add("NM");

        return new Mods()
        {
            Calculation = [.. activeModsCalc],
            Display = [.. activeModsShow]
        };
    }

    internal static double CalculateAccuracy(HitsResult hits, int mode)
    {
        return mode switch
        {
            0 => (double)(100 * ((6 * hits.Hit300) + (2 * hits.Hit100) + hits.Hit50)) / (6 * (hits.Hit50 + hits.Hit100 + hits.Hit300 + hits.HitMiss)),
            1 => (double)(100 * ((2 * hits.Hit300) + hits.Hit100)) / (2 * (hits.Hit300 + hits.Hit100 + hits.HitMiss)),
            2 => (double)(100 * (hits.Hit300 + hits.Hit100 + hits.Hit50)) / (hits.Hit300 + hits.Hit100 + hits.Hit50 + hits.HitKatu + hits.HitMiss),
            3 => (double)(100 * ((6 * hits.HitGeki) + (6 * hits.Hit300) + (4 * hits.HitKatu) + (2 * hits.Hit100) + hits.Hit50)) / (6 * (hits.Hit50 + hits.Hit100 + hits.Hit300 + hits.HitMiss + hits.HitGeki + hits.HitKatu)),
            _ => throw new ArgumentException("Invalid mode provided.")
        };
    }

    internal static Task<int> GetMapMode(string file)
    {
        using var stream = File.OpenRead(file);
        using var reader = new LineBufferedReader(stream);
        int count = 0;
        while (reader.ReadLine() is { } line)
        {
            if (count > 20) return Task.FromResult(0);
            if (line.StartsWith("Mode")) return Task.FromResult(int.Parse(line.Split(':')[1].Trim()));
            count++;
        }

        return Task.FromResult(-1);
    }

    internal static double CalculateAverage(IReadOnlyCollection<int>? array)
    {
        if (array == null || array.Count == 0) return 0;

        var sortedArray = array.OrderBy(x => x).ToArray();

        int count = sortedArray.Length;
        double q1 = sortedArray[(int)(count * 0.25)];
        double q3 = sortedArray[(int)(count * 0.75)];
        double iqr = q3 - q1;

        return sortedArray
            .Where(x => x >= q1 - (1.5 * iqr) && x <= q3 + (1.5 * iqr))
            .Average();
    }

    internal static Dictionary<string, int> GetLeaderBoard(LeaderBoard leaderBoard, int score)
    {
        var currentPositionArray = leaderBoard.Players.ToArray();
        var currentPosition = currentPositionArray.Length + 1;

        if (currentPosition == 1 || !leaderBoard.HasLeaderBoard)
        {
            return new Dictionary<string, int>
            {
                { "currentPosition", 0 },
                { "higherScore", 0 },
                { "highestScore", 0 }
            };
        }

        foreach (var _ in leaderBoard.Players.Where(player => player.Score <= score)) currentPosition--;
        int higherScore = currentPosition - 2 <= 0
            ? leaderBoard.Players[0].Score
            : leaderBoard.Players[currentPosition - 2].Score;

        int highestScore = leaderBoard.Players[0].Score;
        return new Dictionary<string, int>
        {
            { "currentPosition", currentPosition },
            { "higherScore", higherScore },
            { "highestScore", highestScore }
        };
    }

    internal static string GetSongsFolderLocation(string osuFolderDirectory, string customSongsFolder)
    {
        string userName = Environment.UserName;
        string file = Path.Combine(osuFolderDirectory, $"osu!.{userName}.cfg");
        if (!File.Exists(file))
        {
            MessageBox.Show(
                "osu!.Username.cfgが見つからなかったため、Songsフォルダを自動検出できませんでした。\nConfigファイルのSongsFolderを参照します(もし設定されてなかったらデフォルトのSongsフォルダが参照されます。)。",
                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.IsNullOrEmpty(customSongsFolder)
                ? Path.Combine(osuFolderDirectory, "Songs")
                : customSongsFolder;
        }

        foreach (string readLine in File.ReadLines(file))
        {
            if (!readLine.StartsWith("BeatmapDirectory")) continue;
            string path = readLine.Split('=')[1].Trim(' ');
            return path == "Songs" ? Path.Combine(osuFolderDirectory, "Songs") : path;
        }

        FormUtils.ShowErrorMessageBox("osu!.Username.cfgにBeatmapDirectoryが見つからなかったため、Songsフォルダを自動検出できませんでした。\nConfigファイルのSongsFolderを参照します(もし設定されてなかったらデフォルトのSongsフォルダが参照されます。)。");

        return string.IsNullOrEmpty(customSongsFolder) ? Path.Combine(osuFolderDirectory, "Songs") : customSongsFolder;
    }

    internal static double CalculateUnstableRate(IReadOnlyCollection<int> hitErrors)
    {
        if (hitErrors == null || hitErrors.Count == 0) return 0;
        double totalAll = hitErrors.Sum(hit => (long)hit);
        double average = totalAll / hitErrors.Count;
        double variance = hitErrors.Sum(hit => Math.Pow(hit - average, 2)) / hitErrors.Count;
        double unstableRate = Math.Sqrt(variance) * 10;
        return unstableRate > 10000 ? double.NaN : unstableRate;
    }

    internal static string ConvertStatus(OsuMemoryStatus status)
    {
        return status switch
        {
            OsuMemoryStatus.EditingMap => " is editing a beatmap",
            OsuMemoryStatus.GameShutdownAnimation => " is closing osu!",
            OsuMemoryStatus.GameStartupAnimation => " is launching osu!",
            OsuMemoryStatus.MainMenu => " is at the main menu",
            OsuMemoryStatus.MultiplayerRoom => " is in a multiplayer room",
            OsuMemoryStatus.MultiplayerResultsscreen => " is viewing multiplayer results",
            OsuMemoryStatus.MultiplayerSongSelect => " is selecting a song in multiplayer",
            OsuMemoryStatus.NotRunning => " is not running osu!",
            OsuMemoryStatus.OsuDirect => " is browsing osu!direct",
            OsuMemoryStatus.Playing => " is playing a map",
            OsuMemoryStatus.ResultsScreen => " is viewing results",
            OsuMemoryStatus.SongSelect => " is selecting a song",
            OsuMemoryStatus.Unknown => " has an unknown status",
            _ => " has an unknown status"
        };
    }
}
