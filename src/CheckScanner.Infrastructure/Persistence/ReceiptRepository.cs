using CheckScanner.Application.Interfaces;
using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;
using Dapper;
using Npgsql;

namespace CheckScanner.Infrastructure.Persistence;

public sealed class ReceiptRepository(NpgsqlDataSource dataSource) : IReceiptRepository
{
    private const string ReceiptColumns =
        "id, store_name AS StoreName, purchased_at AS PurchasedAt, total, status, created_at AS CreatedAt";

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

        await InsertLineItemsAsync(connection, transaction, receipt.Id, receipt.LineItems, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return receipt.Id;
    }

    public async Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<ReceiptRow>(
            new CommandDefinition(
                $"""
                SELECT {ReceiptColumns}
                FROM receipts
                WHERE id = @Id
                """,
                new { Id = id },
                cancellationToken: cancellationToken));

        if (row is null)
        {
            return null;
        }

        var photos = await connection.QueryAsync<PhotoRow>(
            new CommandDefinition(
                "SELECT id, storage_path AS StoragePath, uploaded_at AS UploadedAt FROM receipt_photos WHERE receipt_id = @Id",
                new { Id = id },
                cancellationToken: cancellationToken));

        var lineItems = await connection.QueryAsync<LineItemRow>(
            new CommandDefinition(
                """
                SELECT id, raw_text AS RawText, description, category, quantity, unit_price AS UnitPrice, line_total AS LineTotal
                FROM receipt_line_items
                WHERE receipt_id = @Id
                ORDER BY position
                """,
                new { Id = id },
                cancellationToken: cancellationToken));

        return ToReceipt(row, photos, lineItems);
    }

    public async Task<IReadOnlyList<Receipt>> GetFlaggedAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var receiptRows = (await connection.QueryAsync<ReceiptRow>(
            new CommandDefinition(
                $"""
                SELECT {ReceiptColumns}
                FROM receipts
                WHERE status = 'Flagged'
                ORDER BY created_at DESC
                """,
                cancellationToken: cancellationToken))).ToList();

        var ids = receiptRows.Select(r => r.Id).ToArray();

        // Photos aren't loaded here -- the Needs Review list doesn't need them.
        var lineItemRows = await connection.QueryAsync<LineItemRow>(
            new CommandDefinition(
                """
                SELECT id, receipt_id AS ReceiptId, raw_text AS RawText, description, category, quantity, unit_price AS UnitPrice, line_total AS LineTotal
                FROM receipt_line_items
                WHERE receipt_id = ANY(@Ids)
                ORDER BY position
                """,
                new { Ids = ids },
                cancellationToken: cancellationToken));

        var lineItemsByReceipt = lineItemRows.GroupBy(l => l.ReceiptId).ToDictionary(g => g.Key, g => g.ToList());

        return receiptRows
            .Select(r => ToReceipt(r, [], lineItemsByReceipt.GetValueOrDefault(r.Id, [])))
            .ToList();
    }

    public async Task UpdateLineItemsAsync(
        Guid receiptId,
        decimal? total,
        ReceiptStatus status,
        IReadOnlyList<ReceiptLineItem> lineItems,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                "UPDATE receipts SET total = @Total, status = @Status WHERE id = @Id",
                new { Id = receiptId, Total = total, Status = status.ToString() },
                transaction,
                cancellationToken: cancellationToken));

        await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM receipt_line_items WHERE receipt_id = @ReceiptId",
                new { ReceiptId = receiptId },
                transaction,
                cancellationToken: cancellationToken));

        await InsertLineItemsAsync(connection, transaction, receiptId, lineItems, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static Task InsertLineItemsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid receiptId,
        IReadOnlyList<ReceiptLineItem> lineItems,
        CancellationToken cancellationToken)
    {
        if (lineItems.Count == 0)
        {
            return Task.CompletedTask;
        }

        var parameters = new DynamicParameters();
        var valueRows = new string[lineItems.Count];
        for (var position = 0; position < lineItems.Count; position++)
        {
            var lineItem = lineItems[position];
            valueRows[position] =
                $"(@Id{position}, @ReceiptId{position}, @Position{position}, @RawText{position}, @Description{position}, @Category{position}, @Quantity{position}, @UnitPrice{position}, @LineTotal{position})";
            parameters.Add($"Id{position}", lineItem.Id);
            parameters.Add($"ReceiptId{position}", receiptId);
            parameters.Add($"Position{position}", position);
            parameters.Add($"RawText{position}", lineItem.RawText);
            parameters.Add($"Description{position}", lineItem.Description);
            parameters.Add($"Category{position}", lineItem.Category);
            parameters.Add($"Quantity{position}", lineItem.Quantity);
            parameters.Add($"UnitPrice{position}", lineItem.UnitPrice);
            parameters.Add($"LineTotal{position}", lineItem.LineTotal);
        }

        return connection.ExecuteAsync(
            new CommandDefinition(
                $"""
                INSERT INTO receipt_line_items (id, receipt_id, position, raw_text, description, category, quantity, unit_price, line_total)
                VALUES {string.Join(", ", valueRows)}
                """,
                parameters,
                transaction,
                cancellationToken: cancellationToken));
    }

    private static Receipt ToReceipt(ReceiptRow row, IEnumerable<PhotoRow> photos, IEnumerable<LineItemRow> lineItems) =>
        new()
        {
            Id = row.Id,
            StoreName = row.StoreName,
            PurchasedAt = AsUtcOffset(row.PurchasedAt),
            Total = row.Total,
            Status = Enum.Parse<ReceiptStatus>(row.Status),
            CreatedAt = AsUtcOffset(row.CreatedAt)!.Value,
            Photos = photos
                .Select(p => new ReceiptPhoto { Id = p.Id, StoragePath = p.StoragePath, UploadedAt = AsUtcOffset(p.UploadedAt)!.Value })
                .ToList(),
            LineItems = lineItems
                .Select(l => new ReceiptLineItem
                {
                    Id = l.Id,
                    RawText = l.RawText,
                    Description = l.Description,
                    Category = l.Category,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal
                })
                .ToList()
        };

    public async Task<IReadOnlyList<Receipt>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var receiptRows = await connection.QueryAsync<ReceiptRow>(
            new CommandDefinition(
                $"""
                SELECT {ReceiptColumns}
                FROM receipts
                ORDER BY created_at DESC
                """,
                cancellationToken: cancellationToken));

        var photoRows = await connection.QueryAsync<PhotoRow>(
            new CommandDefinition(
                "SELECT id, receipt_id AS ReceiptId, storage_path AS StoragePath, uploaded_at AS UploadedAt FROM receipt_photos",
                cancellationToken: cancellationToken));

        var photosByReceipt = photoRows.GroupBy(p => p.ReceiptId).ToDictionary(g => g.Key, g => g.ToList());

        // Line items are not loaded here -- the Receipts list page doesn't need them.
        return receiptRows
            .Select(r => ToReceipt(r, photosByReceipt.GetValueOrDefault(r.Id, []), []))
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

    private sealed class LineItemRow
    {
        public Guid Id { get; set; }
        public Guid ReceiptId { get; set; }
        public string RawText { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
