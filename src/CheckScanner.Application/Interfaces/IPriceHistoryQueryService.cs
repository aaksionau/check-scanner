using CheckScanner.Application.Dtos;

namespace CheckScanner.Application.Interfaces;

public interface IPriceHistoryQueryService
{
    /// <summary>Distinct canonical product names that appear in at least one saved receipt's line items, alphabetical.</summary>
    Task<IReadOnlyList<string>> GetProductNamesAsync(CancellationToken cancellationToken);

    /// <summary>Every (date, store, unit price) point for the given canonical product name, oldest first.</summary>
    Task<IReadOnlyList<PriceHistoryPointDto>> GetPriceHistoryAsync(string productName, CancellationToken cancellationToken);
}
