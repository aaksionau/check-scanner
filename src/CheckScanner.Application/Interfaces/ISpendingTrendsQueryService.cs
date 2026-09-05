using CheckScanner.Application.Dtos;

namespace CheckScanner.Application.Interfaces;

public interface ISpendingTrendsQueryService
{
    /// <summary>Total spend per category, per calendar month, across every saved receipt's line items, oldest month first.</summary>
    Task<IReadOnlyList<CategoryMonthSpendDto>> GetCategorySpendByMonthAsync(CancellationToken cancellationToken);

    /// <summary>The <paramref name="topN"/> canonical products with the highest total spend across every saved receipt, highest first.</summary>
    Task<IReadOnlyList<TopItemSpendDto>> GetTopItemsBySpendAsync(int topN, CancellationToken cancellationToken);
}
