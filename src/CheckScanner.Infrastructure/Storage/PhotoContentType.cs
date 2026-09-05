namespace CheckScanner.Infrastructure.Storage;

/// <summary>Resolves a photo's MIME type from its storage path's extension, for both the vision-LLM call and the HTTP serving endpoint.</summary>
public static class PhotoContentType
{
    public static string Resolve(string storagePath) => Path.GetExtension(storagePath).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".heic" => "image/heic",
        _ => "image/jpeg"
    };
}
