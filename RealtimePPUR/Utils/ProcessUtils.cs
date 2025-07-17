using System.Diagnostics;

namespace RealtimePPUR.Utils;

internal static class ProcessUtils
{
    internal static (bool running, string path) GetOsuProcess()
    {
        Process[] processes = GetProcesses("osu!");
        if (processes.Length == 0) return (false, "");

        Process osuProcess = Process.GetProcessesByName("osu!")[0];
        ProcessModule? osuModule = osuProcess.MainModule;

        if (osuModule == null) return (true, "");

        string? osuDirectory = Path.GetDirectoryName(osuModule.FileName);
        if (osuDirectory == null || !Directory.Exists(osuDirectory)) return (true, "");

        return (true, osuDirectory);
    }

    internal static Process[] GetProcesses(string executableName)
        => Process.GetProcessesByName(executableName);
}
