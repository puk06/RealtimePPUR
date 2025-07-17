using System.Diagnostics;

namespace RealtimePPUR.Utils;

internal static class LogUtils
{
    private const string ErrorFilePath = "Error.log";

    internal static void DebugLogger(string message, bool error = false, bool writeToFile = false)
    {
        string currentDateString = DebugDateGenerator();

        if (error) Console.ForegroundColor = ConsoleColor.Red;

        Debug.WriteLine("[" + currentDateString + "] " + message);
        Console.WriteLine("[" + currentDateString + "] " + message);
        Console.ResetColor();

        if (!writeToFile) return;
        StreamWriter sw = File.Exists(ErrorFilePath) ? File.AppendText(ErrorFilePath) : File.CreateText(ErrorFilePath);
        sw.WriteLine("[" + currentDateString + "]");
        sw.WriteLine(message);
        sw.WriteLine();
        sw.Close();
    }

    private static string DebugDateGenerator()
        => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
}
