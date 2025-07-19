namespace RealtimePPUR.Utils;

internal class ConfigUtils
{
    internal static Dictionary<string, string> ReadConfigFile(string filePath)
    {
        var parameters = new Dictionary<string, string>();
        if (!File.Exists(filePath)) return parameters;

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue; // Skip empty lines and comments
            string[] parts = line.Split('=');
            if (parts.Length != 2) continue;
            string key = parts[0].Trim();
            string value = parts[1].Trim();
            parameters[key] = value;
        }

        return parameters;
    }

    internal static void WriteConfigFile(string filePath, Dictionary<string, string> parameters)
    {
        string[] lines = File.ReadAllLines(filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('=');
            if (parts.Length != 2) continue;

            string key = parts[0].Trim();
            for (int j = 0; j < parameters.Count; j++)
            {
                if (key != parameters.ElementAt(j).Key) continue;
                lines[i] = $"{parameters.ElementAt(j).Key}={parameters.ElementAt(j).Value}";
                break;
            }
        }

        File.WriteAllLines(filePath, lines);
    }

    internal static string ConfigValueToString(bool value) 
        => value ? "true" : "false";

    internal static void SaveConfigFile(Dictionary<string, string> parameters, bool showDialog = true)
    {
        try
        {
            const string filePath = "Config.cfg";
            if (!File.Exists(filePath))
            {
                FormUtils.ShowErrorMessageBox("Config.cfgが見つかりませんでした。RealtimePPURをダウンロードし直してください。");
                return;
            }

            WriteConfigFile(filePath, parameters);
            if (showDialog) FormUtils.ShowInformationMessageBox("Config.cfgの保存が完了しました！");
        }
        catch
        {
            FormUtils.ShowErrorMessageBox("Config.cfgの保存に失敗しました。");
        }
    }
}
