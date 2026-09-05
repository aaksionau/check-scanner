using CheckScanner.Application.Dtos;

namespace CheckScanner.Application.Interfaces;

public interface ISpendingTrendsQueryService
{
    /// <summary>Total spend per category, per calendar month, across every saved receipt's line items, oldest month first.</summary>
    Task<IReadOnlyList<CategoryMonthSpendDto>> GetCategorySpendByMonthAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The <paramref name="topN"/> canonical products with the highest total spend, highest first.
    /// Only receipts purchased on or after <paramref name="from"/> and before or on <paramref name="to"/> are counted;
    /// a null bound is open-ended, and both null covers every saved receipt.
    /// </summary>
    Task<IReadOnlyList<TopItemSpendDto>> GetTopItemsBySpendAsync(int topN, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
}
