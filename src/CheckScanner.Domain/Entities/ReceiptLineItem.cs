namespace CheckScanner.Domain.Entities;

public sealed class ReceiptLineItem
{
    public required Guid Id { get; init; }
    public required string RawText { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal LineTotal { get; init; }
}
