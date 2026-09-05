using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;
using CheckScanner.Domain;
using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;

namespace CheckScanner.Application.Services;

/// <summary>
/// Backs the Needs Review list and the receipt detail/edit view: surfaces
/// receipts that are flagged or failed to parse, loads a single receipt for
/// editing, and re-reconciles on save.
/// </summary>
public sealed class ReceiptReviewService(IReceiptRepository receiptRepository)
{
    public async Task<IReadOnlyList<NeedsReviewReceiptDto>> GetNeedsReviewAsync(CancellationToken cancellationToken)
    {
        var receipts = await receiptRepository.GetNeedsReviewAsync(cancellationToken);
        return receipts
            .Select(r => new NeedsReviewReceiptDto(r.Id, r.StoreName, r.PurchasedAt, r.Total, r.LineItems.Sum(li => li.LineTotal), r.Status))
            .ToList();
    }

    public Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        receiptRepository.GetByIdAsync(id, cancellationToken);

    /// <summary>Re-reconciles the edited values and persists them; returns the persisted line items and resulting status.</summary>
    public async Task<(ReceiptStatus Status, IReadOnlyList<ReceiptLineItem> LineItems)> SaveAsync(
        Guid receiptId,
        decimal? total,
        IReadOnlyList<ReceiptLineItemEditDto> lineItems,
        CancellationToken cancellationToken)
    {
        var entities = lineItems.Select(item => item.ToEntity()).ToList();
        var status = ReceiptReconciler.DetermineStatus(total, entities);
        await receiptRepository.UpdateLineItemsAsync(receiptId, total, status, entities, cancellationToken);
        return (status, entities);
    }
}
