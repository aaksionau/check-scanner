namespace CheckScanner.Web.Components.Shared;

/// <summary>One row in a <see cref="RankedBarList"/>: a label, a formatted value, and a proportional bar width.</summary>
public sealed record RankedBarListItem(string Label, string ValueLabel, double WidthPercent, bool Highlight = false, string? Subtitle = null);
