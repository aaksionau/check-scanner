using CheckScanner.Domain.Enums;

namespace CheckScanner.Application.Dtos;

/// <summary>Row shape for the Needs Review list -- store, date, status, and how far off the receipt is.</summary>
public sealed record NeedsReviewReceiptDto(
    Guid Id,
    string? StoreName,
    DateTimeOffset? PurchasedAt,
    decimal? Total,
    decimal LineItemsSum,
    ReceiptStatus Status)
{
    /// <summary>Null when the receipt has no printed total to compare against (always the case for parse-failed receipts).</summary>
    public decimal? MismatchAmount => Total is { } total ? total - LineItemsSum : null;
}
