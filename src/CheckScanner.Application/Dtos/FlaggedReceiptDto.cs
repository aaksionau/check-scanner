namespace CheckScanner.Application.Dtos;

/// <summary>Row shape for the Needs Review list -- store, date, and how far off the receipt is.</summary>
public sealed record FlaggedReceiptDto(
    Guid Id,
    string? StoreName,
    DateTimeOffset? PurchasedAt,
    decimal? Total,
    decimal LineItemsSum)
{
    /// <summary>Null when the receipt has no printed total to compare against.</summary>
    public decimal? MismatchAmount => Total is { } total ? total - LineItemsSum : null;
}
