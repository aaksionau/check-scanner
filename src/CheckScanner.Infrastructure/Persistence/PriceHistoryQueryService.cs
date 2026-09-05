using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;
using Dapper;
using Npgsql;

namespace CheckScanner.Infrastructure.Persistence;

public sealed class PriceHistoryQueryService(NpgsqlDataSource dataSource) : IPriceHistoryQueryService
{
    public async Task<IReadOnlyList<string>> GetProductNamesAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var names = await connection.QueryAsync<string>(
            new CommandDefinition(
                """
                SELECT DISTINCT description
                FROM receipt_line_items
                ORDER BY description
                """,
                cancellationToken: cancellationToken));

        return names.ToList();
    }

    public async Task<IReadOnlyList<PriceHistoryPointDto>> GetPriceHistoryAsync(string productName, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<PriceHistoryRow>(
            new CommandDefinition(
                """
                SELECT r.purchased_at AS PurchasedAt, r.store_name AS StoreName, li.unit_price AS UnitPrice
                FROM receipt_line_items li
                JOIN receipts r ON r.id = li.receipt_id
                WHERE li.description = @ProductName AND r.purchased_at IS NOT NULL
                ORDER BY r.purchased_at
                """,
                new { ProductName = productName },
                cancellationToken: cancellationToken));

        return rows
            .Select(r => new PriceHistoryPointDto(AsUtcOffset(r.PurchasedAt!.Value), r.StoreName, r.UnitPrice))
            .ToList();
    }

    // Npgsql maps `timestamptz` to plain DateTime (Kind=Utc) by default, not
    // DateTimeOffset -- see ReceiptRepository for the same conversion.
    private static DateTimeOffset AsUtcOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private sealed class PriceHistoryRow
    {
        public DateTime? PurchasedAt { get; set; }
        public string? StoreName { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
