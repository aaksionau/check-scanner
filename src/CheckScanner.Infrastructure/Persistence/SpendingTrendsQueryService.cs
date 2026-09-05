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
            .Select(r => new CategoryMonthSpendDto(DateOnly.FromDateTime(PostgresTimestamp.ToUtcOffset(r.Month)!.Value.Date), r.Category, r.TotalSpend))
            .ToList();
    }

    public async Task<IReadOnlyList<TopItemSpendDto>> GetTopItemsBySpendAsync(int topN, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var fromOffset = from is null ? (DateTimeOffset?)null : new DateTimeOffset(from.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toExclusiveOffset = to is null ? (DateTimeOffset?)null : new DateTimeOffset(to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var rows = await connection.QueryAsync<TopItemSpendDto>(
            new CommandDefinition(
                """
                SELECT li.description AS Description, SUM(li.line_total) AS TotalSpend
                FROM receipt_line_items li
                JOIN receipts r ON r.id = li.receipt_id
                WHERE (@FromOffset::timestamptz IS NULL OR r.purchased_at >= @FromOffset)
                  AND (@ToExclusiveOffset::timestamptz IS NULL OR r.purchased_at < @ToExclusiveOffset)
                GROUP BY li.description
                ORDER BY SUM(li.line_total) DESC
                LIMIT @TopN
                """,
                new { FromOffset = fromOffset, ToExclusiveOffset = toExclusiveOffset, TopN = topN },
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
