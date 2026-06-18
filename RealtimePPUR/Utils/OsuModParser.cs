using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using RealtimePPUR.Models;
using RealtimePPUR.Services.PPCalculation;

namespace RealtimePPUR.Utils;

public static class OsuModParser
{
    private static readonly Dictionary<int, string> modsDict = new()
    {
        { 0, "NM" },
        { 1 << 0, "NF" },
        { 1 << 1, "EZ" },
        { 1 << 2, "TD" },
        { 1 << 3, "HD" },
        { 1 << 4, "HR" },
        { 1 << 5, "SD" },
        { 1 << 6, "DT" },
        { 1 << 7, "RX" },
        { 1 << 8, "HT" },
        { 1 << 9, "NC" },
        { 1 << 10, "FL" },
        { 1 << 11, "AT" },
        { 1 << 12, "SO" },
        { 1 << 13, "RX2" },
        { 1 << 14, "PF" },
        { 1 << 15, "4K" },
        { 1 << 16, "5K" },
        { 1 << 17, "6K" },
        { 1 << 18, "7K" },
        { 1 << 19, "8K" },
        { 1 << 20, "FI" },
        { 1 << 21, "RD" },
        { 1 << 22, "CM" },
        { 1 << 23, "TP" },
        { 1 << 24, "9K" },
        { 1 << 25, "CP" },
        { 1 << 26, "1K" },
        { 1 << 27, "3K" },
        { 1 << 28, "2K" },
        { 1 << 29, "SV2" },
        { 1 << 30, "MR" }
    };

    
    public static readonly Dictionary<OsuGameMode, IEnumerable<Mod>> AvailableModsDictionary = new()
    {
        { OsuGameMode.Osu, PPCalculator.RulesetDictionary[OsuGameMode.Osu].CreateAllMods() },
        { OsuGameMode.Taiko, PPCalculator.RulesetDictionary[OsuGameMode.Taiko].CreateAllMods() },
        { OsuGameMode.Catch, PPCalculator.RulesetDictionary[OsuGameMode.Catch].CreateAllMods() },
        { OsuGameMode.Mania, PPCalculator.RulesetDictionary[OsuGameMode.Mania].CreateAllMods() }
    };

    private static readonly OsuModClassic ClassicMod = new();
    private static readonly string NightCoreString = modsDict[1 << 9];
    private static readonly string DoubleTimeString = modsDict[1 << 6];

    public static string[] ToStrings(int mods)
    {
        List<string> modStrings = new();

        for (int i = 0; i < 32; i++)
        {
            int bit = 1 << i;
            if ((mods & bit) != bit || !modsDict.ContainsKey(bit)) continue;
            modStrings.Add(modsDict[bit]);
        }

        if (modStrings.Contains(NightCoreString) && modStrings.Contains(DoubleTimeString))
        {
            modStrings.Remove(DoubleTimeString);
        }

        return modStrings.ToArray();
    }

    public static Mod[] ToOsuMods(OsuGameMode mode, int mods)
    {
        List<Mod> osuMods = new()
        {
            ClassicMod
        };

        string[] modStrings = ToStrings(mods);
        if (modStrings.Length == 0) return [ClassicMod];

        var availableMods = AvailableModsDictionary[mode];
        foreach (var modString in modStrings)
        {
            var mod = availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, System.StringComparison.CurrentCultureIgnoreCase));
            if (mod != null) osuMods.Add(mod);
        }

        return osuMods.ToArray();
    }
}
