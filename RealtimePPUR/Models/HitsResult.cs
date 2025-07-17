using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;

namespace RealtimePPUR.Models;

internal class HitsResult : IEquatable<HitsResult>
{
    internal int HitGeki { get; set; } = 0;
    internal int Hit300 { get; set; } = 0;
    internal int HitKatu { get; set; } = 0;
    internal int Hit100 { get; set; } = 0;
    internal int Hit50 { get; set; } = 0;
    internal int HitMiss { get; set; } = 0;
    internal int Combo { get; set; } = 0;
    internal int Score { get; set; } = 0;

    internal HitsResult Clone()
    {
        return new HitsResult
        {
            HitGeki = HitGeki,
            Hit300 = Hit300,
            HitKatu = HitKatu,
            Hit100 = Hit100,
            Hit50 = Hit50,
            HitMiss = HitMiss,
            Combo = Combo,
            Score = Score
        };
    }

    internal void SetValueFromMemory(OsuMemoryStatus status, OsuBaseAddresses baseAddresses, bool isPlaying = false)
    {
        if (status == OsuMemoryStatus.Playing || isPlaying)
        {
            HitGeki = baseAddresses.Player.HitGeki;
            Hit300 = baseAddresses.Player.Hit300;
            HitKatu = baseAddresses.Player.HitKatu;
            Hit100 = baseAddresses.Player.Hit100;
            Hit50 = baseAddresses.Player.Hit50;
            HitMiss = baseAddresses.Player.HitMiss;
            Combo = baseAddresses.Player.MaxCombo;
            Score = baseAddresses.Player.Score;
        } 
        else if (status == OsuMemoryStatus.ResultsScreen)
        {
            HitGeki = baseAddresses.ResultsScreen.HitGeki;
            Hit300 = baseAddresses.ResultsScreen.Hit300;
            HitKatu = baseAddresses.ResultsScreen.HitKatu;
            Hit100 = baseAddresses.ResultsScreen.Hit100;
            Hit50 = baseAddresses.ResultsScreen.Hit50;
            HitMiss = baseAddresses.ResultsScreen.HitMiss;
            Combo = baseAddresses.ResultsScreen.MaxCombo;
            Score = baseAddresses.ResultsScreen.Score;
        }
    }

    public override bool Equals(object? obj)
        => obj is HitsResult other && Equals(other);

    public bool Equals(HitsResult? other) => other != null && HitGeki == other.HitGeki && Hit300 == other.Hit300 && HitKatu == other.HitKatu && Hit100 == other.Hit100 && Hit50 == other.Hit50 && HitMiss == other.HitMiss && Combo == other.Combo && Score == other.Score;
    
    internal bool IsEmpty() => HitGeki == 0 && Hit300 == 0 && HitKatu == 0 && Hit100 == 0 && Hit50 == 0 && HitMiss == 0 && Combo == 0 && Score == 0;

    public override int GetHashCode()
        => HashCode.Combine(HitGeki, Hit300, HitKatu, Hit100, Hit50, HitMiss, Combo, Score);

    internal HitsResult GetSimplifiedHits(int mode)
    {
        HitsResult result = new();

        switch (mode)
        {
            case 0:
                result.Hit300 = Hit300;
                result.Hit100 = Hit100 + Hit50;
                result.HitMiss = HitMiss;
                break;
            case 1:
                result.Hit300 = Hit300;
                result.Hit100 = Hit100;
                result.HitMiss = HitMiss;
                break;
            case 2:
                result.Hit300 = Hit300;
                result.Hit100 = Hit100 + Hit50;
                result.HitMiss = HitMiss;
                break;
            case 3:
                result.Hit300 = HitGeki + Hit300;
                result.Hit100 = HitKatu + Hit100 + Hit50;
                result.HitMiss = HitMiss;
                break;
            default:
                throw new ArgumentException("Invalid mode provided. mode -> " + mode);
        }

        return result;
    }
}
