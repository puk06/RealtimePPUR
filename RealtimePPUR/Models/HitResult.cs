using System;

namespace RealtimePPUR.Models;

public sealed class HitResult : IEquatable<HitResult>
{
    public int HitGeki { get; set; } = 0;
    public int Hit300 { get; set; } = 0;
    public int HitKatu { get; set; } = 0;
    public int Hit100 { get; set; } = 0;
    public int Hit50 { get; set; } = 0;
    public int HitMiss { get; set; } = 0;


    public void FromOther(HitResult other)
    {
        HitGeki = other.HitGeki;
        Hit300 = other.Hit300;
        HitKatu = other.HitKatu;
        Hit100 = other.Hit100;
        Hit50 = other.Hit50;
        HitMiss = other.HitMiss;
    }

    public bool Equals(HitResult? other) => other != null && HitGeki == other.HitGeki && Hit300 == other.Hit300 && HitKatu == other.HitKatu && Hit100 == other.Hit100 && Hit50 == other.Hit50 && HitMiss == other.HitMiss;
    public override bool Equals(object? obj) => Equals(obj as HitResult);
    public override int GetHashCode()
    {
        return HashCode.Combine(HitGeki, Hit300, HitKatu, Hit100, Hit50, HitMiss);
    }
}
