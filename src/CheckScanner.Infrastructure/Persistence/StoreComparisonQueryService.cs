using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;
using Dapper;
using Npgsql;

namespace CheckScanner.Infrastructure.Persistence;

public sealed class StoreComparisonQueryService(NpgsqlDataSource dataSource) : IStoreComparisonQueryService
{
    public async Task<IReadOnlyList<StoreLatestPriceDto>> GetLatestPricesByStoreAsync(string productName, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<StoreLatestPriceRow>(
            new CommandDefinition(
                """
                SELECT DISTINCT ON (r.store_name)
                    r.store_name AS StoreName, r.purchased_at AS PurchasedAt, li.unit_price AS UnitPrice
                FROM receipt_line_items li
                JOIN receipts r ON r.id = li.receipt_id
                WHERE li.description = @ProductName AND r.purchased_at IS NOT NULL
                ORDER BY r.store_name, r.purchased_at DESC
                """,
                new { ProductName = productName },
                cancellationToken: cancellationToken));

        return rows
            .Select(r => new StoreLatestPriceDto(r.StoreName, PostgresTimestamp.ToUtcOffset(r.PurchasedAt)!.Value, r.UnitPrice))
            .ToList();
    }

    private sealed class StoreLatestPriceRow
    {
        public string? StoreName { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
