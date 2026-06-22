using System;
using System.IO;

namespace RealtimePPUR.Data;

public static class SystemPath
{
    public static readonly string SoftwareDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RealtimePPUR");
    public static readonly string RuntimeSettingsFilePath = Path.Join(SoftwareDataPath, "Settings", "RuntimeSettings.json");
}
