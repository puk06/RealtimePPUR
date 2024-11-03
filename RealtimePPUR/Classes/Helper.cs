using Octokit;
using osu.Game.IO;
using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtimePPUR.Classes
{
    internal class Helper
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

        public static string RichPresenceStringChecker(string value)
        {
            if (value == null) return "Unknown";
            if (value.Length > 128) value = value[..128];
            return value;
        }

        public static string ConvertStatus(OsuMemoryStatus status)
        {
            return status switch
            {
                OsuMemoryStatus.EditingMap => " is Editing Map",
                OsuMemoryStatus.GameShutdownAnimation => " is Shutting Down osu!",
                OsuMemoryStatus.GameStartupAnimation => " is Starting Up osu!",
                OsuMemoryStatus.MainMenu => " is in Main Menu",
                OsuMemoryStatus.MultiplayerRoom => " is in Multiplayer Room",
                OsuMemoryStatus.MultiplayerResultsscreen => " is in Multiplayer Results",
                OsuMemoryStatus.MultiplayerSongSelect => " is in Multiplayer Song Select",
                OsuMemoryStatus.NotRunning => " is Not Running osu!",
                OsuMemoryStatus.OsuDirect => " is Searching Maps",
                OsuMemoryStatus.Playing => " is Playing Map",
                OsuMemoryStatus.ResultsScreen => " in Results",
                OsuMemoryStatus.SongSelect => " is Selecting Songs",
                OsuMemoryStatus.Unknown => " is Unknown",
                _ => " is Unknown"
            };
        }

        public static string ConvertHits(int mode, HitsResult hits)
        {
            return mode switch
            {
                0 => $"[{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}]",
                1 => $"[{hits.Hit300}/{hits.Hit100}/{hits.HitMiss}]",
                2 => $"[{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}]",
                3 => $"[{hits.HitGeki}/{hits.Hit300}/{hits.HitKatu}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}]",
                _ => $"[{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}]"
            };
        }

        public static Mods ParseMods(int mods)
        {
            List<string> activeModsCalc = new();
            List<string> activeModsShow = new();

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
                Calculate = activeModsCalc.ToArray(),
                Show = activeModsShow.ToArray()
            };
        }

        public static double CalculateAcc(HitsResult hits, int mode)
        {
            return mode switch
            {
                0 => (double)(100 * ((6 * hits.Hit300) + (2 * hits.Hit100) + hits.Hit50)) /
                     (6 * (hits.Hit50 + hits.Hit100 + hits.Hit300 + hits.HitMiss)),
                1 => (double)(100 * ((2 * hits.Hit300) + hits.Hit100)) / (2 * (hits.Hit300 + hits.Hit100 + hits.HitMiss)),
                2 => (double)(100 * (hits.Hit300 + hits.Hit100 + hits.Hit50)) /
                     (hits.Hit300 + hits.Hit100 + hits.Hit50 + hits.HitKatu + hits.HitMiss),
                3 => (double)(100 * ((6 * hits.HitGeki) + (6 * hits.Hit300) + (4 * hits.HitKatu) + (2 * hits.Hit100) +
                                     hits.Hit50)) /
                     (6 * (hits.Hit50 + hits.Hit100 + hits.Hit300 + hits.HitMiss + hits.HitGeki + hits.HitKatu)),
                _ => throw new ArgumentException("Invalid mode provided.")
            };
        }

        public static double IsNaNWithNum(double number) => double.IsNaN(number) ? 0 : number;

        public static Task<int> GetMapMode(string file)
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

        public static double CalculateAverage(IReadOnlyCollection<int> array)
        {
            if (array == null || array.Count == 0) return 0;
            var sortedArray = array.OrderBy(x => x).ToArray();
            int count = sortedArray.Length;
            double q1 = sortedArray[(int)(count * 0.25)];
            double q3 = sortedArray[(int)(count * 0.75)];
            double iqr = q3 - q1;
            var filteredArray = sortedArray.Where(x => x >= q1 - (1.5 * iqr) && x <= q3 + (1.5 * iqr));
            return filteredArray.Average();
        }

        public static Dictionary<string, int> GetLeaderBoard(
            OsuMemoryDataProvider.OsuMemoryModels.Direct.LeaderBoard leaderBoard, int score)
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

        public static string GetSongsFolderLocation(string osuFolderDirectory, string customSongsFolder)
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

            MessageBox.Show(
                "BeatmapDirectoryが見つからなかったため、Songsフォルダを自動検出できませんでした。\nConfigファイルのSongsFolderを参照します(もし設定されてなかったらデフォルトのSongsフォルダが参照されます。)。",
                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.IsNullOrEmpty(customSongsFolder) ? Path.Combine(osuFolderDirectory, "Songs") : customSongsFolder;
        }

        public static double CalculateUnstableRate(IReadOnlyCollection<int> hitErrors)
        {
            if (hitErrors == null || hitErrors.Count == 0) return 0;
            double totalAll = hitErrors.Sum(hit => (long)hit);
            double average = totalAll / hitErrors.Count;
            double variance = hitErrors.Sum(hit => Math.Pow(hit - average, 2)) / hitErrors.Count;
            double unstableRate = Math.Sqrt(variance) * 10;
            return unstableRate > 10000 ? double.NaN : unstableRate;
        }

        public static async void GithubUpdateChecker(string currentVersion)
        {
            try
            {
                var latestRelease = await GetVersion(currentVersion);
                if (latestRelease == currentVersion) return;
                DialogResult result =
                    MessageBox.Show($"最新バージョンがあります！\n\n現在: {currentVersion} \n更新後: {latestRelease}\n\nダウンロードしますか？",
                        "アップデートのお知らせ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result != DialogResult.Yes) return;

                if (!File.Exists("./Updater/Software Updater.exe"))
                {
                    MessageBox.Show("アップデーターが見つかりませんでした。手動でダウンロードしてください。", "エラー", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                string updaterPath = Path.GetFullPath("./Updater/Software Updater.exe");
                const string author = "puk06";
                const string repository = "RealtimePPUR";
                const string executableName = "RealtimePPUR";
                ProcessStartInfo args = new()
                {
                    FileName = $"\"{updaterPath}\"",
                    Arguments = $"\"{latestRelease}\" \"{author}\" \"{repository}\" \"{executableName}\" \"Config.cfg\"",
                    UseShellExecute = true
                };

                Process.Start(args);
            }
            catch (Exception exception)
            {
                MessageBox.Show("アップデートチェック中にエラーが発生しました" + exception.Message, "エラー", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public static async Task<string> GetVersion(string currentVersion)
        {
            try
            {
                var releaseType = currentVersion.Split('-')[1];
                var githubClient = new GitHubClient(new ProductHeaderValue("RealtimePPUR"));
                var tags = await githubClient.Repository.GetAllTags("puk06", "RealtimePPUR");
                string latestVersion = currentVersion;
                foreach (var tag in tags)
                {
                    if (releaseType == "Release")
                    {
                        if (tag.Name.Split("-")[1] != "Release") continue;
                        latestVersion = tag.Name;
                        break;
                    }

                    latestVersion = tag.Name;
                    break;
                }

                return latestVersion;
            }
            catch
            {
                throw new Exception("アップデートの取得に失敗しました");
            }
        }

        public static void ToggleChecked(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                menuItem.Checked = !menuItem.Checked;
                DebugLogger($"{menuItem.Text} is now {menuItem.Checked}");
            }
        }

        public static void WriteConfigFile(string filePath, Dictionary<string, string> parameters)
        {
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split('=');
                if (parts.Length != 2) continue;

                string key = parts[0].Trim();
                for (int j = 0; j < parameters.Count; j++)
                {
                    if (key != parameters.ElementAt(j).Key) continue;
                    lines[i] = $"{parameters.ElementAt(j).Key}={parameters.ElementAt(j).Value}";
                    break;
                }
            }
            File.WriteAllLines(filePath, lines);
        }

        public static string ConfigValueToString(bool value) => value ? "true" : "false";

        public static void ShowErrorMessageBox(string message) =>
            MessageBox.Show(message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static void ShowInformationMessageBox(string message) =>
            MessageBox.Show(message, "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void DebugLogger(string message, bool error = false)
        {
            if (error)
            {
                Debug.WriteLine("[" + DateTime.Now + "] " + message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[" + DateTime.Now + "] " + message);
                Console.ResetColor();
                return;
            }
            Debug.WriteLine("[" + DateTime.Now + "] " + message);
            Console.WriteLine("[" + DateTime.Now + "] " + message);
        }
    }
}
