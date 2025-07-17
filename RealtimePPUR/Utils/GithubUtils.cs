using Octokit;
using System.Diagnostics;

namespace RealtimePPUR.Utils;

internal class GithubUtils
{
    internal static async void CheckUpdate(string currentVersion)
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
                FormUtils.ShowErrorMessageBox("アップデーターが見つかりませんでした。手動でダウンロードしてください。");
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
            FormUtils.ShowErrorMessageBox("アップデートチェック中にエラーが発生しました" + exception.Message);
        }
    }

    internal static async Task<string> GetVersion(string currentVersion)
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
}
