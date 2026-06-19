using System.IO;

namespace RealtimePPUR.Models;

public class OsuMapInfo
{
    public string FolderName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string RelativeBeatmapPath => Path.Combine(FolderName, FileName); // For Check
}
