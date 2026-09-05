namespace CheckScanner.Application.Dtos;

/// <summary>
/// Shape a future IReceiptParser implementation will return. Not produced or
/// consumed by anything yet -- it exists so the interface below has a
/// concrete return type to define against.
/// </summary>
public sealed record ParsedReceiptDto(
    string? StoreName,
    DateTimeOffset? PurchasedAt,
    decimal? Total,
    IReadOnlyList<ParsedLineItemDto> LineItems);

public sealed record ParsedLineItemDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);
