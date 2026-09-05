using CheckScanner.Application.Dtos;

namespace CheckScanner.Web.Components.Shared;

/// <summary>
/// Pure geometry: turns category-spend-by-month rows into a stacked bar
/// chart's SVG coordinates, one bar per month and one stacked segment per
/// category. No I/O, mirrors <see cref="PriceHistoryChartBuilder"/>.
/// </summary>
public static class SpendingTrendsChartBuilder
{
    public const int Width = 320;
    public const int Height = 200;
    private const int PaddingTop = 12;
    private const int PaddingBottom = 24;
    private const int PaddingSides = 8;
    private const int BarGap = 8;

    public sealed record Segment(string Category, string Color, double Y, double Height, decimal Amount);

    public sealed record Bar(string MonthLabel, IReadOnlyList<Segment> Segments, double X, double Width);

    public sealed record Legend(string Category, string Color);

    public sealed record ChartResult(IReadOnlyList<Bar> Bars, IReadOnlyList<Legend> Legend);

    /// <summary>Caller must pass a non-empty list.</summary>
    public static ChartResult Build(IReadOnlyList<CategoryMonthSpendDto> rows)
    {
        var categories = rows.Select(r => r.Category).Distinct().OrderBy(c => c).ToList();
        var colorByCategory = categories
            .Select((category, index) => (category, color: ChartPalette.Colors[index % ChartPalette.Colors.Length]))
            .ToDictionary(x => x.category, x => x.color);

        var rowsByMonth = rows.GroupBy(r => r.Month).ToDictionary(g => g.Key, g => g.OrderBy(r => r.Category).ToList());
        var months = rowsByMonth.Keys.OrderBy(m => m).ToList();
        var maxTotal = rowsByMonth.Values.Select(monthRows => monthRows.Sum(r => r.TotalSpend)).DefaultIfEmpty(0m).Max();

        var plotHeight = Height - PaddingTop - PaddingBottom;
        var plotWidth = Width - 2 * PaddingSides;
        var barWidth = months.Count == 0 ? 0 : (plotWidth - BarGap * (months.Count - 1)) / (double)months.Count;

        double ScaleHeight(decimal amount) =>
            maxTotal == 0 ? 0 : (double)amount / (double)maxTotal * plotHeight;

        var bars = months.Select((month, index) =>
        {
            var x = PaddingSides + index * (barWidth + BarGap);
            var monthRows = rowsByMonth[month];

            double cursorY = Height - PaddingBottom;
            var segments = new List<Segment>();
            foreach (var row in monthRows)
            {
                var segmentHeight = ScaleHeight(row.TotalSpend);
                cursorY -= segmentHeight;
                segments.Add(new Segment(row.Category, colorByCategory[row.Category], cursorY, segmentHeight, row.TotalSpend));
            }

            return new Bar(month.ToString("MMM yy"), segments, x, barWidth);
        }).ToList();

        var legend = categories.Select(c => new Legend(c, colorByCategory[c])).ToList();

        return new ChartResult(bars, legend);
    }
}
