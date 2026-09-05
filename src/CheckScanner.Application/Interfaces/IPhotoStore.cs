namespace CheckScanner.Application.Interfaces;

/// <summary>
/// Persists an uploaded receipt photo's bytes and hands back a storage path/key
/// that <see cref="IReceiptRepository"/> can record and a future viewer can
/// resolve back to the file.
/// </summary>
public interface IPhotoStore
{
    Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken);

    /// <summary>Reopens a previously-saved photo by the path <see cref="SaveAsync"/> returned.</summary>
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);
}
