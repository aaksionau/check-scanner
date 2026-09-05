using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;
using Dapper;
using Npgsql;

namespace CheckScanner.Infrastructure.Persistence;

public sealed class SpendingTrendsQueryService(NpgsqlDataSource dataSource) : ISpendingTrendsQueryService
{
    public async Task<IReadOnlyList<CategoryMonthSpendDto>> GetCategorySpendByMonthAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<CategoryMonthSpendRow>(
            new CommandDefinition(
                """
                SELECT date_trunc('month', r.purchased_at) AS Month, li.category AS Category, SUM(li.line_total) AS TotalSpend
                FROM receipt_line_items li
                JOIN receipts r ON r.id = li.receipt_id
                WHERE r.purchased_at IS NOT NULL
                GROUP BY date_trunc('month', r.purchased_at), li.category
                ORDER BY date_trunc('month', r.purchased_at)
                """,
                cancellationToken: cancellationToken));

        return rows
            .Select(r => new CategoryMonthSpendDto(DateOnly.FromDateTime(r.Month), r.Category, r.TotalSpend))
            .ToList();
    }

    public async Task<IReadOnlyList<TopItemSpendDto>> GetTopItemsBySpendAsync(int topN, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<TopItemSpendDto>(
            new CommandDefinition(
                """
                SELECT description AS Description, SUM(line_total) AS TotalSpend
                FROM receipt_line_items
                GROUP BY description
                ORDER BY SUM(line_total) DESC
                LIMIT @TopN
                """,
                new { TopN = topN },
                cancellationToken: cancellationToken));

        return rows.ToList();
    }

    private sealed class CategoryMonthSpendRow
    {
        public DateTime Month { get; set; }
        public string Category { get; set; } = "";
        public decimal TotalSpend { get; set; }
    }
}
