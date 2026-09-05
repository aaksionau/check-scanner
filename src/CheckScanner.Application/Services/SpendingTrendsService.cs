using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;

namespace CheckScanner.Application.Services;

/// <summary>Backs the Spending Trends page: category spend per month, and top items by total spend.</summary>
public sealed class SpendingTrendsService(ISpendingTrendsQueryService spendingTrendsQueryService)
{
    public Task<IReadOnlyList<CategoryMonthSpendDto>> GetCategorySpendByMonthAsync(CancellationToken cancellationToken) =>
        spendingTrendsQueryService.GetCategorySpendByMonthAsync(cancellationToken);

    public Task<IReadOnlyList<TopItemSpendDto>> GetTopItemsBySpendAsync(int topN, DateOnly? from, DateOnly? to, CancellationToken cancellationToken) =>
        spendingTrendsQueryService.GetTopItemsBySpendAsync(topN, from, to, cancellationToken);
}
