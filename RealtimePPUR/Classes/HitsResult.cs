namespace RealtimePPUR.Classes
{
    public class HitsResult
    {
        public int HitGeki { get; set; }
        public int Hit300 { get; set; }
        public int HitKatu { get; set; }
        public int Hit100 { get; set; }
        public int Hit50 { get; set; }
        public int HitMiss { get; set; }
        public int Combo { get; set; }
        public int Score { get; set; }
        public HitsResult Clone()
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
        public bool Equals(HitsResult other) => other != null && HitGeki == other.HitGeki && Hit300 == other.Hit300 && HitKatu == other.HitKatu && Hit100 == other.Hit100 && Hit50 == other.Hit50 && HitMiss == other.HitMiss && Combo == other.Combo && Score == other.Score;
        public bool IsEmpty() => HitGeki == 0 && Hit300 == 0 && HitKatu == 0 && Hit100 == 0 && Hit50 == 0 && HitMiss == 0 && Combo == 0 && Score == 0;
    }
}
