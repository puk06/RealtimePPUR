namespace RealtimePPUR.Utils;

internal static class DiscordRichPresenceUtils
{

    internal static string CheckString(string value)
    {
        if (value == null) return "Unknown";
        if (value.Length > 128) value = value[..128];
        return value;
    }
}
