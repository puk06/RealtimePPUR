using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using Octokit;

namespace RealtimePPUR.Updater
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("バージョン情報が取得できませんでした。");
                    Thread.Sleep(3000);
                    return;
                }

                var CurrentVersion = args[0];

                if (string.IsNullOrEmpty(CurrentVersion))
                {
                    Console.WriteLine("バージョン情報が取得できませんでした。");
                    Thread.Sleep(3000);
                }

                Console.WriteLine("アップデートを確認します。");

                var latestVersion = GithubUpdateChecker(CurrentVersion).Result;

                if (latestVersion == CurrentVersion)
                {
                    Console.WriteLine("最新バージョンです。");
                    Thread.Sleep(3000);
                    return;
                }

                Console.WriteLine($"最新バージョンが見つかりました({CurrentVersion} → {latestVersion})");
                Console.WriteLine("Configファイルはバックアップを取らない限りリセットされてしまいます。もし大丈夫な場合はEnterを押してください。");
                Console.ReadLine();
                Console.WriteLine("RealtimePPUR関係のソフトをすべて終了します。");

                var processes = Process.GetProcessesByName("RealtimePPUR");
                foreach (var process in processes)
                {
                    process.Kill();
                }

                Console.WriteLine("RealtimePPURを終了しました。アップデートを開始します。");
                var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                var updater = new Updater(latestVersion, arch);

                await updater.Update();

                Console.WriteLine("アップデートが完了しました！ソフトを使ってくれてありがとうございます！");
                Thread.Sleep(3000);
            }
            catch (Exception e)
            {
                Console.WriteLine("アップデート中にエラーが発生しました: " + e.Message);
                Thread.Sleep(3000);
            }
        }

        private static async Task<string> GithubUpdateChecker(string currentVersion)
        {
            var latestRelease = await GetVersion(currentVersion);
            return latestRelease == currentVersion ? currentVersion : latestRelease;
        }

        private static async Task<string> GetVersion(string currentVersion)
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
    }

    internal class Updater
    {
        private readonly string _version;
        private readonly string _arch;

        public Updater(string version, string arch)
        {
            this._version = version;
            this._arch = arch;
        }

        private const string baseurl = "https://github.com/puk06/RealtimePPUR/releases/download/";

        public async Task Update()
        {
            var downloadUrl = $"{baseurl}{_version}/RealtimePPUR-{_arch}.zip";
            var tempPath = Path.GetTempPath();
            var tempFile = Path.Combine(tempPath, "RealtimePPUR.zip");
            var extractPath = Path.Combine(tempPath, "RealtimePPUR.Temp");

            Console.WriteLine("ファイルのダウンロードを開始しています...");
            Console.WriteLine("ファイルのダウンロード中です...ネットの環境などにより時間がかかる可能性があります。");

            using var client = new WebClient();
            await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempFile);

            Console.WriteLine("ダウンロードが完了しました。");
            Console.WriteLine("ファイルの展開中です...");

            ZipFile.ExtractToDirectory(tempFile, extractPath, Encoding.UTF8, true);
            File.Delete(tempFile);

            var files = Directory.GetFiles(extractPath);
            var softwarePath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var fileName = Path.GetFileName(file);
                var currentFile = Path.Combine(softwarePath, fileName);
                Console.WriteLine($"ファイルのコピー中です... {i + 1}/{files.Length}: {fileName}");
                File.Copy(file, currentFile, true);
            }
            Directory.Delete(extractPath, true);
        }
    }
}

