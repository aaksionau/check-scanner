using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;

namespace CheckScanner.Application.Services;

/// <summary>Backs the Item Price History and Store Comparison pages: the shared product picker, per-store price-history points, and latest price per store.</summary>
public sealed class PriceHistoryService(IPriceHistoryQueryService priceHistoryQueryService)
{
    public Task<IReadOnlyList<string>> GetProductNamesAsync(CancellationToken cancellationToken) =>
        priceHistoryQueryService.GetProductNamesAsync(cancellationToken);

    public Task<IReadOnlyList<PriceHistoryPointDto>> GetPriceHistoryAsync(string productName, CancellationToken cancellationToken) =>
        priceHistoryQueryService.GetPriceHistoryAsync(productName, cancellationToken);

    public Task<IReadOnlyList<StoreLatestPriceDto>> GetLatestPricesByStoreAsync(string productName, CancellationToken cancellationToken) =>
        priceHistoryQueryService.GetLatestPricesByStoreAsync(productName, cancellationToken);
}
