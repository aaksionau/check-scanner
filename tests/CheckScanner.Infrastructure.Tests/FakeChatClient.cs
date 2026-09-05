using Microsoft.Extensions.AI;

namespace CheckScanner.Infrastructure.Tests;

/// <summary>
/// Stands in for a real Azure AI Foundry-backed IChatClient in tests. Returns a
/// canned response so ReceiptParsingAgent's structured-output binding runs for
/// real against known text, with no network call.
/// </summary>
public sealed class FakeChatClient(string responseText) : IChatClient
{
    public IReadOnlyList<ChatMessage>? LastMessages { get; private set; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        LastMessages = messages.ToList();
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
