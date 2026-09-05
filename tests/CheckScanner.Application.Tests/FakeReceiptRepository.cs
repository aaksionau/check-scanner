using CheckScanner.Application.Interfaces;
using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;

namespace CheckScanner.Application.Tests;

public sealed class FakeReceiptRepository : IReceiptRepository
{
    private readonly Dictionary<Guid, Receipt> _receipts = [];

    public FakeReceiptRepository(params Receipt[] seed)
    {
        foreach (var receipt in seed)
        {
            _receipts[receipt.Id] = receipt;
        }
    }

    public Task<Guid> AddAsync(Receipt receipt, CancellationToken cancellationToken)
    {
        _receipts[receipt.Id] = receipt;
        return Task.FromResult(receipt.Id);
    }

    public Task<IReadOnlyList<Receipt>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Receipt>>(_receipts.Values.OrderByDescending(r => r.CreatedAt).ToList());

    public Task<IReadOnlyList<Receipt>> GetNeedsReviewAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Receipt>>(
            _receipts.Values
                .Where(r => r.Status is ReceiptStatus.Flagged or ReceiptStatus.ParseFailed)
                .OrderByDescending(r => r.CreatedAt)
                .ToList());

    public Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_receipts.GetValueOrDefault(id));

    public Task UpdateLineItemsAsync(
        Guid receiptId,
        decimal? total,
        ReceiptStatus status,
        IReadOnlyList<ReceiptLineItem> lineItems,
        CancellationToken cancellationToken)
    {
        _receipts[receiptId] = _receipts[receiptId] with { Total = total, Status = status, LineItems = lineItems };
        return Task.CompletedTask;
    }
}
