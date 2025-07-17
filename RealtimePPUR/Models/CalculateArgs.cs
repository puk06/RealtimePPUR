namespace RealtimePPUR.Models;

internal class CalculateArgs
{
    internal double Accuracy { get; set; } = 100;
    internal int Combo { get; set; }
    internal int Score { get; set; }
    internal string[] Mods { get; set; } = [];
    internal int? Time { get; set; }
    internal bool CalculateBeforePlaying { get; set; }
}
