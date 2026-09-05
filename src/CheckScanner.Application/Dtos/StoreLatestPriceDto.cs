namespace CheckScanner.Application.Dtos;

public sealed record StoreLatestPriceDto(string? StoreName, DateTimeOffset PurchasedAt, decimal UnitPrice);
