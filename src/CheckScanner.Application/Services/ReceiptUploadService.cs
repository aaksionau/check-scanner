using CheckScanner.Application.Interfaces;
using CheckScanner.Domain;
using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CheckScanner.Application.Services;

/// <summary>
/// Single entry point for the Upload page: saves each photo's bytes, parses
/// them into line items, reconciles the result against the printed total, and
/// persists one Receipt row linking all of it together.
/// </summary>
public sealed class ReceiptUploadService(
    IPhotoStore photoStore,
    IReceiptParser receiptParser,
    IReceiptRepository receiptRepository,
    ILogger<ReceiptUploadService> logger)
{
    public async Task<Guid> UploadAsync(
        IReadOnlyList<(string FileName, Stream Content)> photos, CancellationToken cancellationToken)
    {
        if (photos.Count == 0)
        {
            throw new ArgumentException("At least one photo is required.", nameof(photos));
        }

        var now = DateTimeOffset.UtcNow;
        var uploadedPhotos = new List<ReceiptPhoto>(photos.Count);
        foreach (var (fileName, content) in photos)
        {
            var storagePath = await photoStore.SaveAsync(fileName, content, cancellationToken);
            uploadedPhotos.Add(new ReceiptPhoto
            {
                Id = Guid.NewGuid(),
                StoragePath = storagePath,
                UploadedAt = now
            });
        }

        var receipt = await BuildReceiptAsync(uploadedPhotos, now, cancellationToken);
        return await receiptRepository.AddAsync(receipt, cancellationToken);
    }

    private async Task<Receipt> BuildReceiptAsync(
        IReadOnlyList<ReceiptPhoto> uploadedPhotos, DateTimeOffset now, CancellationToken cancellationToken)
    {
        try
        {
            var parsed = await receiptParser.ParseAsync(
                uploadedPhotos.Select(p => p.StoragePath).ToList(), cancellationToken);

            var lineItems = parsed.LineItems
                .Select(item => new ReceiptLineItem
                {
                    Id = Guid.NewGuid(),
                    RawText = item.RawText,
                    Description = item.Description,
                    Category = item.Category,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                })
                .ToList();

            var isFlagged = ReceiptReconciler.Reconcile(parsed.Total, lineItems);

            return new Receipt
            {
                Id = Guid.NewGuid(),
                StoreName = parsed.StoreName,
                PurchasedAt = parsed.PurchasedAt,
                Total = parsed.Total,
                Status = isFlagged ? ReceiptStatus.Flagged : ReceiptStatus.Parsed,
                CreatedAt = now,
                Photos = uploadedPhotos,
                LineItems = lineItems
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse uploaded receipt photos.");

            return new Receipt
            {
                Id = Guid.NewGuid(),
                StoreName = null,
                PurchasedAt = null,
                Total = null,
                Status = ReceiptStatus.ParseFailed,
                CreatedAt = now,
                Photos = uploadedPhotos,
                LineItems = []
            };
        }
    }
}
