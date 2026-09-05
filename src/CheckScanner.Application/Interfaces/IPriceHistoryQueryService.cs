using CheckScanner.Application.Dtos;

namespace CheckScanner.Application.Interfaces;

public interface IPriceHistoryQueryService
{
    /// <summary>Distinct canonical product names that appear in at least one saved receipt's line items, alphabetical.</summary>
    Task<IReadOnlyList<string>> GetProductNamesAsync(CancellationToken cancellationToken);

    /// <summary>Every (date, store, unit price) point for the given canonical product name, oldest first.</summary>
    Task<IReadOnlyList<PriceHistoryPointDto>> GetPriceHistoryAsync(string productName, CancellationToken cancellationToken);

    /// <summary>Most recent (date, unit price) per store that has sold the given canonical product name. Stores with no purchase history for it are omitted.</summary>
    Task<IReadOnlyList<StoreLatestPriceDto>> GetLatestPricesByStoreAsync(string productName, CancellationToken cancellationToken);
}
