using CheckScanner.Domain.Entities;

namespace CheckScanner.Domain;

/// <summary>
/// Decides whether a parsed receipt's line items reconcile with its printed
/// total, within a tolerance that absorbs tax/rounding drift. Pure, no I/O.
/// </summary>
public static class ReceiptReconciler
{
    private const decimal ToleranceAmount = 0.05m;

    /// <summary>True if the receipt should be flagged for manual review.</summary>
    public static bool Reconcile(decimal? printedTotal, IReadOnlyList<ReceiptLineItem> lineItems)
    {
        if (printedTotal is not { } total)
        {
            return true;
        }

        var summed = lineItems.Sum(item => item.LineTotal);
        return Math.Abs(summed - total) > ToleranceAmount;
    }
}
