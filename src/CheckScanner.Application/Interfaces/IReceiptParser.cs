using CheckScanner.Application.Dtos;

namespace CheckScanner.Application.Interfaces;

/// <summary>
/// Parses 1..N photos -- ordered segments of one logical receipt -- into
/// structured line items via a vision-capable LLM (Azure AI Foundry gpt-4o-mini).
/// </summary>
public interface IReceiptParser
{
    Task<ParsedReceiptDto> ParseAsync(IReadOnlyList<string> photoStoragePaths, CancellationToken cancellationToken);
}
