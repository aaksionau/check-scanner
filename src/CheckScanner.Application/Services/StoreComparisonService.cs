using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;

namespace CheckScanner.Application.Services;

/// <summary>Backs the Store Comparison page: reuses the Item Price History product picker, plus per-store latest prices.</summary>
public sealed class StoreComparisonService(
    IPriceHistoryQueryService priceHistoryQueryService,
    IStoreComparisonQueryService storeComparisonQueryService)
{
    public Task<IReadOnlyList<string>> GetProductNamesAsync(CancellationToken cancellationToken) =>
        priceHistoryQueryService.GetProductNamesAsync(cancellationToken);

    public Task<IReadOnlyList<StoreLatestPriceDto>> GetLatestPricesByStoreAsync(string productName, CancellationToken cancellationToken) =>
        storeComparisonQueryService.GetLatestPricesByStoreAsync(productName, cancellationToken);
}
