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
    public void WithinRealisticTaxRate_IsNotFlagged()
    {
        var lineItems = new[] { LineItem(100.00m) };

        // 7% sales tax on top of the line-item sum.
        var isFlagged = ReceiptReconciler.Reconcile(107.00m, lineItems);

        Assert.False(isFlagged);
    }

    [Fact]
    public void ExceedsRealisticTaxRate_IsFlagged()
    {
        var lineItems = new[] { LineItem(100.00m) };

        // 25% gap is far beyond any plausible combined sales tax rate.
        var isFlagged = ReceiptReconciler.Reconcile(125.00m, lineItems);

        Assert.True(isFlagged);
    }

    [Fact]
    public void RoundingDriftWithinCushion_IsNotFlagged()
    {
        var lineItems = new[] { LineItem(12.01m) };

        var isFlagged = ReceiptReconciler.Reconcile(12.00m, lineItems);

        Assert.False(isFlagged);
    }

    [Fact]
    public void LineItemsExceedTotalBeyondCushion_IsFlagged()
    {
        var lineItems = new[] { LineItem(12.05m) };

        // Line items summing above the printed total isn't explained by tax.
        var isFlagged = ReceiptReconciler.Reconcile(12.00m, lineItems);

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
