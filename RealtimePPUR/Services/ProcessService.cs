using System.Diagnostics;
using System.IO;

namespace RealtimePPUR.Services;

public static class ProcessService
{
    public static ProcessStatus GetProcessInfo(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        if (processes.Length == 0) return new ProcessStatus(false, string.Empty, null);

        var process = Process.GetProcessesByName(processName)[0];
        var processModule = process.MainModule;

        if (processModule == null) return new ProcessStatus(false, string.Empty, null);

        var processDirectory = Path.GetDirectoryName(processModule.FileName);
        if (processDirectory == null || !Directory.Exists(processDirectory)) return new ProcessStatus(false, string.Empty, null);

        return new ProcessStatus(false, processDirectory, process);
    }
}

public record ProcessStatus(bool IsRunning, string Path, Process? Process);
