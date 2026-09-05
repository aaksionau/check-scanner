namespace CheckScanner.Infrastructure.Options;

public sealed class FoundryOptions
{
    public const string SectionName = "AzureFoundry";

    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
    public required string DeploymentName { get; set; }
}
