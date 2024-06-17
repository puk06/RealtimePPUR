using System;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace RealtimePPUR
{

    internal static class Program
    {

        [STAThread]
        private static async Task Main()
        {
            try
            {
                await CheckFiles();
                CultureInfo.CurrentCulture = new CultureInfo("en-us");
                CultureInfo.CurrentUICulture = new CultureInfo("en-us");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new RealtimePpur());
            }
            catch (Exception softwareError)
            {
                MessageBox.Show($"ソフトの起動に失敗しました。\nエラー内容: {softwareError.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task CheckFiles()
        {
            if (!Directory.Exists("Updater") || !File.Exists("config.cfg") || !Directory.Exists("src"))
            {
                MessageBox.Show("起動に必要なファイルが見つかりませんでした。ダウンロードを開始します。\nこの作業はすぐ終わるのでソフトは開いたままにしておいてください！終わったらソフトが自動で起動します！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await new Updater().DownloadFiles();
                MessageBox.Show("ダウンロードが完了しました！ソフトを起動します！", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    internal class Updater
    {
        private const string SrcVersion = "v1.0.0";
        private const string Baseurl = "https://github.com/puk06/RealtimePPUR-src/releases/download/";

        public async Task DownloadFiles()
        {
            const string downloadUrl = $"{Baseurl}{SrcVersion}/RealtimePPUR-src.zip";
            var tempPath = Path.GetTempPath();
            var tempFile = Path.Combine(tempPath, "RealtimePPUR-src.zip");
            var extractPath = Path.Combine(tempPath, "RealtimePPUR-src.Temp");

            using var client = new WebClient();
            await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempFile);

            ZipFile.ExtractToDirectory(tempFile, extractPath, Encoding.UTF8, true);
            File.Delete(tempFile);

            var folders = Directory.GetDirectories(extractPath);
            var files = Directory.GetFiles(extractPath);


            var softwarePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (softwarePath == null) throw new DirectoryNotFoundException("ソフトのフォルダの取得に失敗しました。");

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var currentFile = Path.Combine(softwarePath, fileName);
                if (File.Exists(currentFile)) continue;
                File.Copy(file, currentFile, true);
            }

            foreach (var folder in folders)
            {
                var folderName = Path.GetFileName(folder);
                var currentFolder = Path.Combine(softwarePath, folderName);
                if (!Directory.Exists(currentFolder)) Directory.CreateDirectory(currentFolder);
                DirectoryCopy(folder, currentFolder, true);
            }

            Directory.Delete(extractPath, true);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists) throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            if (!Directory.Exists(destDirName)) Directory.CreateDirectory(destDirName);


            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            if (!copySubDirs) return;
            foreach (var subdir in dirs)
            {
                var tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, true);
            }
        }
    }
}
