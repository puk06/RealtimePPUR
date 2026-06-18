namespace RealtimePPUR.Models;

public class HitResult
{
    public int HitGeki { get; set; } = 0;
    public int Hit300 { get; set; } = 0;
    public int HitKatu { get; set; } = 0;
    public int Hit100 { get; set; } = 0;
    public int Hit50 { get; set; } = 0;
    public int HitMiss { get; set; } = 0;

    public bool Equals(HitResult? other) => other != null && HitGeki == other.HitGeki && Hit300 == other.Hit300 && HitKatu == other.HitKatu && Hit100 == other.Hit100 && Hit50 == other.Hit50 && HitMiss == other.HitMiss;

    public void FromOther(HitResult other)
    {
        HitGeki = other.HitGeki;
        Hit300 = other.Hit300;
        HitKatu = other.HitKatu;
        Hit100 = other.Hit100;
        Hit50 = other.Hit50;
        HitMiss = other.HitMiss;
    }
}
