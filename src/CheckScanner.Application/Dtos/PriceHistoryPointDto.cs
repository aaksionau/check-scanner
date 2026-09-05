namespace CheckScanner.Application.Dtos;

public sealed record PriceHistoryPointDto(DateTimeOffset PurchasedAt, string? StoreName, decimal UnitPrice);
