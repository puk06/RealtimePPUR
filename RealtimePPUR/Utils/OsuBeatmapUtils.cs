using System.IO;
using System.Threading.Tasks;
using osu.Game.IO;
using RealtimePPUR.Models;

namespace RealtimePPUR.Utils;

public static class OsuBeatmapUtils
{
    public static Task<OsuGameMode> GetMapMode(string file)
    {
        using var stream = File.OpenRead(file);
        using var reader = new LineBufferedReader(stream);
        int count = 0;
        while (reader.ReadLine() is { } line)
        {
            if (count > 20) return Task.FromResult(OsuGameMode.Osu);
            if (line.StartsWith("Mode")) return Task.FromResult((OsuGameMode)int.Parse(line.Split(':')[1].Trim()));
            count++;
        }

        return Task.FromResult(OsuGameMode.None);
    }

    public static string GetMapPath(string? songs, string? folder, string? file)
    {
        if (string.IsNullOrEmpty(songs) || string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(file)) return string.Empty;

        string beatmapPath = Path.Combine(songs, folder, file);

        if (!File.Exists(beatmapPath))
        {
            beatmapPath = Path.Combine(songs, folder.Trim(), file);

            if (!File.Exists(beatmapPath))
            {
                beatmapPath = Path.Combine(songs, folder, file.Trim());
            }

            if (!File.Exists(beatmapPath))
            {
                beatmapPath = Path.Combine(songs, folder.Trim(), file.Trim());
            }
        }

        if (!File.Exists(beatmapPath)) return string.Empty;

        return beatmapPath;
    }
}
