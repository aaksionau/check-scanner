using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;
using CheckScanner.Domain;
using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;

namespace CheckScanner.Application.Services;

/// <summary>
/// Backs the Needs Review list and the receipt detail/edit view: surfaces
/// flagged receipts, loads a single receipt for editing, and re-reconciles
/// on save.
/// </summary>
public sealed class ReceiptReviewService(IReceiptRepository receiptRepository)
{
    public async Task<IReadOnlyList<FlaggedReceiptDto>> GetFlaggedAsync(CancellationToken cancellationToken)
    {
        var receipts = await receiptRepository.GetFlaggedAsync(cancellationToken);
        return receipts
            .Select(r => new FlaggedReceiptDto(r.Id, r.StoreName, r.PurchasedAt, r.Total, r.LineItems.Sum(li => li.LineTotal)))
            .ToList();
    }

    public Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        receiptRepository.GetByIdAsync(id, cancellationToken);

    /// <summary>Re-reconciles the edited values and persists them; returns the resulting status.</summary>
    public async Task<ReceiptStatus> SaveAsync(
        Guid receiptId,
        decimal? total,
        IReadOnlyList<ReceiptLineItemEditDto> lineItems,
        CancellationToken cancellationToken)
    {
        var entities = lineItems
            .Select(item => new ReceiptLineItem
            {
                Id = item.Id ?? Guid.NewGuid(),
                RawText = item.RawText,
                Description = item.Description,
                Category = item.Category,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            })
            .ToList();

        var status = ReceiptReconciler.Reconcile(total, entities) ? ReceiptStatus.Flagged : ReceiptStatus.Parsed;
        await receiptRepository.UpdateLineItemsAsync(receiptId, total, status, entities, cancellationToken);
        return status;
    }
}
