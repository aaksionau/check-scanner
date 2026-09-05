using CheckScanner.Application.Interfaces;
using CheckScanner.Domain.Entities;
using CheckScanner.Domain.Enums;

namespace CheckScanner.Application.Services;

/// <summary>
/// Single entry point for the Upload page: saves each photo's bytes, then
/// persists one Receipt row (no line items -- real parsing is a follow-up
/// slice) linking all of them together.
/// </summary>
public sealed class ReceiptUploadService(IPhotoStore photoStore, IReceiptRepository receiptRepository)
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

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            StoreName = null,
            PurchasedAt = null,
            Total = null,
            Status = ReceiptStatus.Uploaded,
            CreatedAt = now,
            Photos = uploadedPhotos,
            LineItems = []
        };

        return await receiptRepository.AddAsync(receipt, cancellationToken);
    }
}
