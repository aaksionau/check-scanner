using CheckScanner.Infrastructure.Parsing;
using Microsoft.Extensions.AI;

namespace CheckScanner.Infrastructure.Tests;

public class ReceiptParsingAgentTests
{
    [Fact]
    public async Task WellFormedSinglePhotoResponse_ParsesIntoDto()
    {
        var chatClient = new FakeChatClient("""
            {
                "storeName": "Trader Joe's",
                "purchasedAt": "2026-08-30T00:00:00+00:00",
                "total": 12.50,
                "lineItems": [
                    { "rawText": "ORG BANANA", "description": "Organic Bananas", "category": "Produce", "quantity": 1, "unitPrice": 12.50, "lineTotal": 12.50 }
                ]
            }
            """);
        var agent = new ReceiptParsingAgent(chatClient, new FakePhotoStore());

        var result = await agent.ParseAsync(["receipt-1.jpg"], CancellationToken.None);

        Assert.Equal("Trader Joe's", result.StoreName);
        Assert.Equal(12.50m, result.Total);
        Assert.Single(result.LineItems);
        Assert.Equal("Organic Bananas", result.LineItems[0].Description);
        Assert.Equal("Produce", result.LineItems[0].Category);
    }

    [Fact]
    public async Task MultiPhotoResponse_SendsAllPhotosInOneCall()
    {
        var chatClient = new FakeChatClient("""
            {
                "storeName": "Costco",
                "purchasedAt": "2026-08-30T00:00:00+00:00",
                "total": 40.00,
                "lineItems": [
                    { "rawText": "ITEM A", "description": "Item A", "category": "General", "quantity": 1, "unitPrice": 15.00, "lineTotal": 15.00 },
                    { "rawText": "ITEM B", "description": "Item B", "category": "General", "quantity": 1, "unitPrice": 25.00, "lineTotal": 25.00 }
                ]
            }
            """);
        var agent = new ReceiptParsingAgent(chatClient, new FakePhotoStore());

        var result = await agent.ParseAsync(["receipt-1.jpg", "receipt-2.jpg"], CancellationToken.None);

        Assert.Equal(2, result.LineItems.Count);
        var sentMessage = Assert.Single(chatClient.LastMessages!);
        var imageContents = sentMessage.Contents.OfType<DataContent>().ToList();
        Assert.Equal(2, imageContents.Count);
    }

    [Fact]
    public async Task MalformedResponse_Throws()
    {
        var chatClient = new FakeChatClient("Sorry, I can't read this receipt.");
        var agent = new ReceiptParsingAgent(chatClient, new FakePhotoStore());

        await Assert.ThrowsAnyAsync<Exception>(() => agent.ParseAsync(["receipt-1.jpg"], CancellationToken.None));
    }
}
