namespace CheckScanner.Web.Components.Shared;

/// <summary>Shared scaling arithmetic for chart builders and ranked bar lists.</summary>
public static class ChartMath
{
    /// <summary>Ratio of <paramref name="value"/> to <paramref name="max"/>, as 0..1. Zero when <paramref name="max"/> is zero.</summary>
    public static double RatioOf(decimal value, decimal max) =>
        max == 0 ? 0 : (double)(value / max);
}
