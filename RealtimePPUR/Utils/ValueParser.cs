namespace RealtimePPUR.Utils;

public static class ValueParser
{
    public static int Int(string? value, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return int.TryParse(value, out int v) ? v : defaultValue;
    }

    public static double Double(string? value, double defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return double.TryParse(value, out double v) ? v : defaultValue;
    }

    public static bool Boolean(string? value, bool defaultValue = false)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return value == "1";
    }
}
