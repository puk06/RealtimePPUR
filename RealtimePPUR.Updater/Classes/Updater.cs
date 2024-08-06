using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;

namespace RealtimePPUR.Updater.Classes
{
    public class Updater
    {
        private readonly string version;
        private readonly string arch;

        public Updater(string version, string arch)
        {
            this.version = version;
            this.arch = arch;
        }

        private const string BASEURL = "https://github.com/puk06/RealtimePPUR/releases/download/";

        public async Task Update()
        {
            var downloadUrl = $"{BASEURL}{version}/RealtimePPUR-{arch}.zip";
            var tempPath = Path.GetTempPath();
            var tempFile = Path.Combine(tempPath, "RealtimePPUR.zip");
            var extractPath = Path.Combine(tempPath, "RealtimePPUR.Temp");

            Console.WriteLine("ファイルのダウンロードを開始しています...");
            Console.WriteLine("ファイルのダウンロード中です...ソフトを終了しないでください！");

            using var client = new WebClient();
            await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempFile);

            Console.WriteLine("ダウンロードが完了しました！");
            Console.WriteLine("ファイルの展開中です...");

            ZipFile.ExtractToDirectory(tempFile, extractPath, Encoding.UTF8, true);
            File.Delete(tempFile);

            var folders = Directory.GetDirectories(extractPath);
            folders = folders.Where(x => !x.Contains("Updater")).ToArray();
            var files = Directory.GetFiles(extractPath);

            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currentPath == null)
            {
                Console.WriteLine("カレントフォルダの取得に失敗しました。");
                Thread.Sleep(3000);
                return;
            }
            var softwarePath = Directory.GetParent(currentPath)?.FullName;
            if (softwarePath == null)
            {
                Console.WriteLine("ソフトウェアのフォルダの取得に失敗しました。");
                Thread.Sleep(3000);
                return;
            }

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var fileName = Path.GetFileName(file);
                var currentFile = Path.Combine(softwarePath, fileName);
                Console.WriteLine($"ファイルのコピー中です... {i + 1}/{files.Length}: {fileName}");
                File.Copy(file, currentFile, true);
            }

            for (int i = 0; i < folders.Length; i++)
            {
                var folder = folders[i];
                var folderName = Path.GetFileName(folder);
                var currentFolder = Path.Combine(softwarePath, folderName);
                if (!Directory.Exists(currentFolder)) Directory.CreateDirectory(currentFolder);
                Console.WriteLine($"フォルダのコピー中です... {i + 1}/{folders.Length}: {folderName}");
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
