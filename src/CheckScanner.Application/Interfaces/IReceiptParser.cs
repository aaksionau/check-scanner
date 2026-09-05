using CheckScanner.Application.Dtos;

namespace CheckScanner.Application.Interfaces;

/// <summary>
/// Seam for the vision-model line-item parser (Azure AI Foundry gpt-4o-mini).
/// No implementation exists yet -- this walking skeleton only defines the
/// shape a later slice will build against.
/// </summary>
public interface IReceiptParser
{
    Task<ParsedReceiptDto> ParseAsync(IReadOnlyList<string> photoStoragePaths, CancellationToken cancellationToken);
}
