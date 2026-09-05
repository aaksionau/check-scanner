using CheckScanner.Application.Interfaces;
using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;
using Dapper;
using Npgsql;

namespace CheckScanner.Infrastructure.Persistence;

public sealed class ReceiptRepository(NpgsqlDataSource dataSource) : IReceiptRepository
{
    public async Task<Guid> AddAsync(Receipt receipt, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO receipts (id, store_name, purchased_at, total, status, created_at)
                VALUES (@Id, @StoreName, @PurchasedAt, @Total, @Status, @CreatedAt)
                """,
                new
                {
                    receipt.Id,
                    receipt.StoreName,
                    receipt.PurchasedAt,
                    receipt.Total,
                    Status = receipt.Status.ToString(),
                    receipt.CreatedAt
                },
                transaction,
                cancellationToken: cancellationToken));

        foreach (var photo in receipt.Photos)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO receipt_photos (id, receipt_id, storage_path, uploaded_at)
                    VALUES (@Id, @ReceiptId, @StoragePath, @UploadedAt)
                    """,
                    new
                    {
                        photo.Id,
                        ReceiptId = receipt.Id,
                        photo.StoragePath,
                        photo.UploadedAt
                    },
                    transaction,
                    cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
        return receipt.Id;
    }

    public async Task<IReadOnlyList<Receipt>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var receiptRows = await connection.QueryAsync<ReceiptRow>(
            new CommandDefinition(
                """
                SELECT id, store_name AS StoreName, purchased_at AS PurchasedAt, total, status, created_at AS CreatedAt
                FROM receipts
                ORDER BY created_at DESC
                """,
                cancellationToken: cancellationToken));

        var photoRows = await connection.QueryAsync<PhotoRow>(
            new CommandDefinition(
                "SELECT id, receipt_id AS ReceiptId, storage_path AS StoragePath, uploaded_at AS UploadedAt FROM receipt_photos",
                cancellationToken: cancellationToken));

        var photosByReceipt = photoRows
            .GroupBy(p => p.ReceiptId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ReceiptPhoto>)g
                .Select(p => new ReceiptPhoto { Id = p.Id, StoragePath = p.StoragePath, UploadedAt = AsUtcOffset(p.UploadedAt)!.Value })
                .ToList());

        return receiptRows
            .Select(r => new Receipt
            {
                Id = r.Id,
                StoreName = r.StoreName,
                PurchasedAt = AsUtcOffset(r.PurchasedAt),
                Total = r.Total,
                Status = Enum.Parse<ReceiptStatus>(r.Status),
                CreatedAt = AsUtcOffset(r.CreatedAt)!.Value,
                Photos = photosByReceipt.GetValueOrDefault(r.Id, []),
                LineItems = []
            })
            .ToList();
    }

    // Npgsql maps `timestamptz` to plain DateTime (Kind=Utc) by default, not
    // DateTimeOffset -- Dapper's constructor-based materialization can't
    // implicitly convert between the two, so the *Row types below stay in
    // DateTime and get converted here instead.
    private static DateTimeOffset? AsUtcOffset(DateTime? value) =>
        value is null ? null : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));

    // Dapper materializes these via reflection (public settable properties),
    // not the constructor -- that's the path that tolerates the
    // DateTime/DateTimeOffset and nullable/non-nullable differences between
    // what Npgsql returns and what the Domain entities use.
    private sealed class ReceiptRow
    {
        public Guid Id { get; set; }
        public string? StoreName { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public decimal? Total { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    private sealed class PhotoRow
    {
        public Guid Id { get; set; }
        public Guid ReceiptId { get; set; }
        public string StoragePath { get; set; } = "";
        public DateTime UploadedAt { get; set; }
    }
}
