using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;

namespace CheckScanner.Application.Interfaces;

public interface IReceiptRepository
{
    Task<Guid> AddAsync(Receipt receipt, CancellationToken cancellationToken);

    /// <summary>Newest first, with photos loaded. Line items are not loaded.</summary>
    Task<IReadOnlyList<Receipt>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Newest first, with line items loaded (needed to compute each one's mismatch amount). Photos are not loaded.</summary>
    Task<IReadOnlyList<Receipt>> GetFlaggedAsync(CancellationToken cancellationToken);

    /// <summary>Loads a single receipt with its photos and line items, or null if it doesn't exist.</summary>
    Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Replaces a receipt's line items wholesale and updates its total/status.</summary>
    Task UpdateLineItemsAsync(
        Guid receiptId,
        decimal? total,
        ReceiptStatus status,
        IReadOnlyList<ReceiptLineItem> lineItems,
        CancellationToken cancellationToken);
}
