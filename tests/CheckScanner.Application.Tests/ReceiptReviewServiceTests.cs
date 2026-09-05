using CheckScanner.Application.Dtos;
using CheckScanner.Application.Services;
using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;

namespace CheckScanner.Application.Tests;

public class ReceiptReviewServiceTests
{
    [Fact]
    public async Task GetNeedsReviewAsync_IncludesFlaggedAndParseFailedReceipts()
    {
        var flagged = Receipt(ReceiptStatus.Flagged);
        var parseFailed = Receipt(ReceiptStatus.ParseFailed);
        var parsed = Receipt(ReceiptStatus.Parsed);
        var service = new ReceiptReviewService(new FakeReceiptRepository(flagged, parseFailed, parsed));

        var result = await service.GetNeedsReviewAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == flagged.Id);
        Assert.Contains(result, r => r.Id == parseFailed.Id);
        Assert.DoesNotContain(result, r => r.Id == parsed.Id);
    }

    [Fact]
    public async Task GetNeedsReviewAsync_ParseFailedReceipt_HasNoMismatchAmount()
    {
        var parseFailed = Receipt(ReceiptStatus.ParseFailed, total: null, lineItems: []);
        var service = new ReceiptReviewService(new FakeReceiptRepository(parseFailed));

        var result = await service.GetNeedsReviewAsync(CancellationToken.None);

        Assert.Null(Assert.Single(result).MismatchAmount);
    }

    [Fact]
    public async Task GetNeedsReviewAsync_FlaggedReceipt_ReportsMismatchAgainstPrintedTotal()
    {
        var lineItems = new[] { LineItem(10.00m) };
        var flagged = Receipt(ReceiptStatus.Flagged, total: 15.00m, lineItems: lineItems);
        var service = new ReceiptReviewService(new FakeReceiptRepository(flagged));

        var result = await service.GetNeedsReviewAsync(CancellationToken.None);

        Assert.Equal(5.00m, Assert.Single(result).MismatchAmount);
    }

    [Fact]
    public async Task SaveAsync_ReconciledEdit_ReturnsParsedStatus()
    {
        var receipt = Receipt(ReceiptStatus.ParseFailed, total: null, lineItems: []);
        var service = new ReceiptReviewService(new FakeReceiptRepository(receipt));

        var (status, savedLineItems) = await service.SaveAsync(receipt.Id, 4.00m, MilkEdits, CancellationToken.None);

        Assert.Equal(ReceiptStatus.Parsed, status);
        Assert.Single(savedLineItems);
    }

    [Fact]
    public async Task SaveAsync_StillMismatched_ReturnsFlaggedStatusAndPersists()
    {
        var receipt = Receipt(ReceiptStatus.ParseFailed, total: null, lineItems: []);
        var repository = new FakeReceiptRepository(receipt);
        var service = new ReceiptReviewService(repository);

        var (status, _) = await service.SaveAsync(receipt.Id, 40.00m, MilkEdits, CancellationToken.None);

        Assert.Equal(ReceiptStatus.Flagged, status);
        var persisted = await repository.GetByIdAsync(receipt.Id, CancellationToken.None);
        Assert.Equal(ReceiptStatus.Flagged, persisted!.Status);
    }

    private static Receipt Receipt(ReceiptStatus status, decimal? total = null, IReadOnlyList<ReceiptLineItem>? lineItems = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            StoreName = "Store",
            PurchasedAt = DateTimeOffset.UtcNow,
            Total = total,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            Photos = [],
            LineItems = lineItems ?? []
        };

    private static readonly ReceiptLineItemEditDto[] MilkEdits =
        [new() { Description = "Milk", Category = "Dairy", Quantity = 1, UnitPrice = 4.00m, LineTotal = 4.00m }];

    private static ReceiptLineItem LineItem(decimal lineTotal) => new()
    {
        Id = Guid.NewGuid(),
        RawText = "raw",
        Description = "item",
        Category = "General",
        Quantity = 1,
        UnitPrice = lineTotal,
        LineTotal = lineTotal
    };
}
