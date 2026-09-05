using CheckScanner.Domain.Enums;

namespace CheckScanner.Domain.Entities;

public sealed class Receipt
{
    public required Guid Id { get; init; }
    public string? StoreName { get; init; }
    public DateTimeOffset? PurchasedAt { get; init; }
    public decimal? Total { get; init; }
    public required ReceiptStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required IReadOnlyList<ReceiptPhoto> Photos { get; init; }
    public required IReadOnlyList<ReceiptLineItem> LineItems { get; init; }
}
