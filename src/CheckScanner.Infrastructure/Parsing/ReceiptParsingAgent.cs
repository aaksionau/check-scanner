using CheckScanner.Application.Dtos;
using CheckScanner.Application.Interfaces;
using CheckScanner.Infrastructure.Storage;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace CheckScanner.Infrastructure.Parsing;

/// <summary>
/// Parses a receipt's photos via a Microsoft Agent Framework agent backed by
/// the Azure AI Foundry gpt-4o-mini deployment, using structured-output
/// binding to deserialize directly into <see cref="ParsedReceiptDto"/>.
/// </summary>
public sealed class ReceiptParsingAgent : IReceiptParser
{
    private const string Instructions = """
        You read grocery store receipts from photos. You may be given multiple photos that are
        ordered segments of one single logical receipt (e.g. a long receipt that didn't fit in one
        frame) -- treat them together as one receipt, not as separate receipts.

        Extract the store name (normalized to a canonical form, e.g. without a store number or
        branch suffix), the purchase date, and the printed total. Extract every line item printed
        on the receipt, not only groceries -- include all items such as household goods. For each
        line item, extract the raw printed text, a normalized canonical product name, a category,
        the quantity, the unit price, and the line total.
        """;

    private readonly AIAgent _agent;
    private readonly IPhotoStore _photoStore;

    public ReceiptParsingAgent(IChatClient chatClient, IPhotoStore photoStore)
    {
        _agent = chatClient.AsAIAgent(instructions: Instructions, name: "ReceiptParsingAgent");
        _photoStore = photoStore;
    }

    public async Task<ParsedReceiptDto> ParseAsync(IReadOnlyList<string> photoStoragePaths, CancellationToken cancellationToken)
    {
        var photoBytes = await Task.WhenAll(
            photoStoragePaths.Select(path => ReadAllBytesAsync(path, cancellationToken)));

        var contents = new List<AIContent>
        {
            new TextContent("Extract the structured receipt data from the following photo(s), in order.")
        };
        for (var i = 0; i < photoStoragePaths.Count; i++)
        {
            contents.Add(new DataContent(photoBytes[i], PhotoContentType.Resolve(photoStoragePaths[i])));
        }

        var message = new ChatMessage(ChatRole.User, contents);
        var response = await _agent.RunAsync<ParsedReceiptDto>(message, cancellationToken: cancellationToken);
        return response.Result;
    }

    private async Task<byte[]> ReadAllBytesAsync(string storagePath, CancellationToken cancellationToken)
    {
        await using var stream = await _photoStore.OpenReadAsync(storagePath, cancellationToken);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }
}
