using System;

namespace RealtimePPUR.Classes
{
    public class CalculateArgs
    {
        public double Accuracy { get; set; } = 100;
        public int Combo { get; set; }
        public int Score { get; set; }
        public bool NoClassicMod { get; set; }
        public string[] Mods { get; set; } = Array.Empty<string>();
        public int? Time { get; set; }
        public bool CalculateBeforePlaying { get; set; }
    }
}
