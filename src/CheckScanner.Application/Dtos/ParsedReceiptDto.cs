namespace CheckScanner.Application.Dtos;

/// <summary>Structured output an IReceiptParser implementation binds a vision-LLM response into.</summary>
public sealed record ParsedReceiptDto(
    string? StoreName,
    DateTimeOffset? PurchasedAt,
    decimal? Total,
    IReadOnlyList<ParsedLineItemDto> LineItems);

public sealed record ParsedLineItemDto(
    string RawText,
    string Description,
    string Category,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);
