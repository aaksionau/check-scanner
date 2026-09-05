using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;

namespace CheckScanner.Application.Services;

/// <summary>Backs the Item Price History page: the product picker and per-store price-history points.</summary>
public sealed class PriceHistoryService(IPriceHistoryQueryService priceHistoryQueryService)
{
    public Task<IReadOnlyList<string>> GetProductNamesAsync(CancellationToken cancellationToken) =>
        priceHistoryQueryService.GetProductNamesAsync(cancellationToken);

    public Task<IReadOnlyList<PriceHistoryPointDto>> GetPriceHistoryAsync(string productName, CancellationToken cancellationToken) =>
        priceHistoryQueryService.GetPriceHistoryAsync(productName, cancellationToken);
}
