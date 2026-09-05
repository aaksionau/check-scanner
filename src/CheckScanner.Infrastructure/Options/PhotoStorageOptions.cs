namespace CheckScanner.Infrastructure.Options;

public sealed class PhotoStorageOptions
{
    public const string SectionName = "PhotoStorage";

    public required string BasePath { get; set; }
}
