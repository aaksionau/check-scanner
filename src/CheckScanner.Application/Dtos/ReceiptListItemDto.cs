using CheckScanner.Domain.Enums;

namespace CheckScanner.Application.Dtos;

public sealed record ReceiptListItemDto(
    Guid Id,
    string? StoreName,
    DateTimeOffset? PurchasedAt,
    decimal? Total,
    ReceiptStatus Status);
