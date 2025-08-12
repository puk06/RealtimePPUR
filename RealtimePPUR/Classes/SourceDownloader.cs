using System.IO.Compression;
using System.Text;

namespace RealtimePPUR.Classes;

internal static class SourceDownloader
{
    private const string SRC_VERSION = "v1.0.8";
    private const string BASEURL = "https://github.com/puk06/RealtimePPUR-src/releases/download/";

    public static async Task DownloadFiles()
    {
        const string downloadUrl = $"{BASEURL}{SRC_VERSION}/RealtimePPUR-src.zip";
        var tempPath = Path.GetTempPath();
        var tempFile = Path.Combine(tempPath, "RealtimePPUR-src.zip");
        var extractPath = Path.Combine(tempPath, "RealtimePPUR-src.Temp");

        using (var httpClient = new HttpClient())
        {
            using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);
        }

        ZipFile.ExtractToDirectory(tempFile, extractPath, Encoding.UTF8, true);
        File.Delete(tempFile);

        var folders = Directory.GetDirectories(extractPath);
        var files = Directory.GetFiles(extractPath);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (File.Exists(fileName)) continue;
            File.Copy(file, fileName, true);
        }

        foreach (var folder in folders)
        {
            var folderName = Path.GetFileName(folder);
            if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
            DirectoryCopy(folder, folderName, true);
        }

        Directory.Delete(extractPath, true);
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        var dir = new DirectoryInfo(sourceDirName);
        var dirs = dir.GetDirectories();

        if (!dir.Exists) throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
        if (!Directory.Exists(destDirName) && destDirName != null)
            Directory.CreateDirectory(destDirName);

        var files = dir.GetFiles();
        foreach (var file in files)
        {
            if (destDirName == null) continue;
            var tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true);
        }

        if (!copySubDirs) return;
        foreach (var subdir in dirs)
        {
            if (destDirName == null) continue;
            var tempPath = Path.Combine(destDirName, subdir.Name);
            DirectoryCopy(subdir.FullName, tempPath, true);
        }
    }
}
