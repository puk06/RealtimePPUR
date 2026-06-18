using System.IO;

namespace RealtimePPUR.Models;

public class OsuMapInfo
{
    public string FolderName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    // TODO: パスが壊れている時のバグを直す。詳細はRealtimePPUR、538行目
    public string FullPath(string songs) => Path.Combine(songs, FolderName, FileName);
}
