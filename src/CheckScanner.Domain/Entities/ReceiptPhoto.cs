namespace CheckScanner.Domain.Entities;

public sealed class ReceiptPhoto
{
    public required Guid Id { get; init; }
    public required string StoragePath { get; init; }
    public required DateTimeOffset UploadedAt { get; init; }
}
