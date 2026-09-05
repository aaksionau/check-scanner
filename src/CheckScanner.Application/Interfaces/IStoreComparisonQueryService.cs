using CheckScanner.Application.Dtos;

namespace CheckScanner.Application.Interfaces;

public interface IStoreComparisonQueryService
{
    /// <summary>Most recent (date, unit price) per store that has sold the given canonical product name. Stores with no purchase history for it are omitted.</summary>
    Task<IReadOnlyList<StoreLatestPriceDto>> GetLatestPricesByStoreAsync(string productName, CancellationToken cancellationToken);
}
