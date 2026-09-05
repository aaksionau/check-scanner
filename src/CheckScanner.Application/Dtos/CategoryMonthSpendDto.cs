namespace CheckScanner.Application.Dtos;

public sealed record CategoryMonthSpendDto(DateOnly Month, string Category, decimal TotalSpend);
