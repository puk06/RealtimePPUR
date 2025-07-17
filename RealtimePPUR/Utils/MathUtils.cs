namespace RealtimePPUR.Utils;

internal class MathUtils
{
    internal static double IsNaNWithNum(double? number)
    {
        if (number is null) return 0.0;
        return double.IsNaN(number.Value) ? 0.0 : number.Value;
    }
}
