using CheckScanner.Application.Interfaces;
using CheckScanner.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CheckScanner.Infrastructure.Storage;

/// <summary>Writes uploaded photos to a PVC-backed directory on disk.</summary>
public sealed class FileSystemPhotoStore(IOptions<PhotoStorageOptions> options) : IPhotoStore
{
    public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken)
    {
        var basePath = options.Value.BasePath;
        Directory.CreateDirectory(basePath);

        var storedFileName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(basePath, storedFileName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storedFileName;
    }
}
