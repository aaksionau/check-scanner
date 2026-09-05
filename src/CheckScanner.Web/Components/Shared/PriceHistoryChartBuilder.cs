using CheckScanner.Application.Dtos;

namespace CheckScanner.Web.Components.Shared;

/// <summary>
/// Pure geometry: turns price-history points into scaled SVG coordinates, one
/// series per store. No I/O, so it's a candidate for the reconciler-style
/// unit tests this repo favors for standalone logic, should this chart grow
/// more branching than the current min/max scaling.
/// </summary>
public static class PriceHistoryChartBuilder
{
    public const int Width = 320;
    public const int Height = 160;
    private const int Padding = 28;

    private static readonly string[] Palette =
        ["#2563eb", "#dc2626", "#16a34a", "#d97706", "#7c3aed", "#0891b2", "#db2777", "#65a30d"];

    public sealed record Series(string StoreName, string Color, string PolylinePoints, IReadOnlyList<(double X, double Y)> Points);

    public sealed record ChartResult(
        IReadOnlyList<Series> Series,
        string MinDateLabel,
        string MaxDateLabel,
        string MinPriceLabel,
        string MaxPriceLabel);

    /// <summary>Caller must pass a non-empty list.</summary>
    public static ChartResult Build(IReadOnlyList<PriceHistoryPointDto> points)
    {
        var minDate = points.Min(p => p.PurchasedAt);
        var maxDate = points.Max(p => p.PurchasedAt);
        var minPrice = points.Min(p => p.UnitPrice);
        var maxPrice = points.Max(p => p.UnitPrice);

        var dateRangeTicks = (maxDate - minDate).Ticks;
        var priceRange = maxPrice - minPrice;

        double ScaleX(DateTimeOffset date) =>
            dateRangeTicks == 0
                ? Width / 2.0
                : Padding + (double)(date - minDate).Ticks / dateRangeTicks * (Width - 2 * Padding);

        double ScaleY(decimal price) =>
            priceRange == 0
                ? Height / 2.0
                : Height - Padding - (double)(price - minPrice) / (double)priceRange * (Height - 2 * Padding);

        var seriesByStore = points
            .GroupBy(p => ReceiptDisplayFormat.StoreName(p.StoreName))
            .OrderBy(g => g.Key)
            .Select((group, index) =>
            {
                var ordered = group.OrderBy(p => p.PurchasedAt).ToList();
                var coords = ordered.Select(p => (X: ScaleX(p.PurchasedAt), Y: ScaleY(p.UnitPrice))).ToList();
                var polyline = string.Join(" ", coords.Select(c => $"{c.X:F1},{c.Y:F1}"));
                var color = Palette[index % Palette.Length];
                return new Series(group.Key, color, polyline, coords);
            })
            .ToList();

        return new ChartResult(
            seriesByStore,
            minDate.ToString("MMM d, yyyy"),
            maxDate.ToString("MMM d, yyyy"),
            minPrice.ToString("C"),
            maxPrice.ToString("C"));
    }
}
