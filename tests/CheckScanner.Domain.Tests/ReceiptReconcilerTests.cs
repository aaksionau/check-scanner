using CheckScanner.Domain.Entities;

namespace CheckScanner.Domain.Tests;

public class ReceiptReconcilerTests
{
    [Fact]
    public void ExactMatch_IsNotFlagged()
    {
        var lineItems = new[] { LineItem(12.00m) };

        var isFlagged = ReceiptReconciler.Reconcile(12.00m, lineItems);

        Assert.False(isFlagged);
    }

    [Fact]
    public void WithinTolerance_IsNotFlagged()
    {
        var lineItems = new[] { LineItem(12.00m) };

        var isFlagged = ReceiptReconciler.Reconcile(12.04m, lineItems);

        Assert.False(isFlagged);
    }

    [Fact]
    public void JustOutsideTolerance_IsFlagged()
    {
        var lineItems = new[] { LineItem(12.00m) };

        var isFlagged = ReceiptReconciler.Reconcile(12.06m, lineItems);

        Assert.True(isFlagged);
    }

    [Fact]
    public void ZeroLineItems_IsFlagged()
    {
        var isFlagged = ReceiptReconciler.Reconcile(12.00m, []);

        Assert.True(isFlagged);
    }

    [Fact]
    public void NegativeDiscountLine_ReconcilesAgainstNetTotal()
    {
        var lineItems = new[] { LineItem(12.00m), LineItem(-2.00m) };

        var isFlagged = ReceiptReconciler.Reconcile(10.00m, lineItems);

        Assert.False(isFlagged);
    }

    [Fact]
    public void MissingPrintedTotal_IsFlagged()
    {
        var lineItems = new[] { LineItem(12.00m) };

        var isFlagged = ReceiptReconciler.Reconcile(null, lineItems);

        Assert.True(isFlagged);
    }

    private static ReceiptLineItem LineItem(decimal lineTotal) => new()
    {
        Id = Guid.NewGuid(),
        RawText = "raw",
        Description = "item",
        Category = "General",
        Quantity = 1,
        UnitPrice = lineTotal,
        LineTotal = lineTotal
    };
}
