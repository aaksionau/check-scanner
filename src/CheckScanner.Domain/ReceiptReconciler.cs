using CheckScanner.Domain.Entities;

namespace CheckScanner.Domain;

/// <summary>
/// Decides whether a parsed receipt's line items reconcile with its printed
/// total. Sales tax is never extracted as its own line item, so it shows up
/// as the printed total exceeding the line-item sum; tolerance for that
/// direction scales with the sum to cover realistic tax rates. Line items
/// summing to more than the printed total isn't explained by tax, so that
/// direction only gets a small rounding cushion. Pure, no I/O.
/// </summary>
public static class ReceiptReconciler
{
    private const decimal RoundingCushion = 0.02m;
    private const decimal MaxTaxRate = 0.15m;

    /// <summary>True if the receipt should be flagged for manual review.</summary>
    public static bool Reconcile(decimal? printedTotal, IReadOnlyList<ReceiptLineItem> lineItems)
    {
        if (printedTotal is not { } total)
        {
            return true;
        }

        var summed = lineItems.Sum(item => item.LineTotal);
        var difference = total - summed;

        if (difference < -RoundingCushion)
        {
            return true;
        }

        var maxExpectedTax = (summed * MaxTaxRate) + RoundingCushion;
        return difference > maxExpectedTax;
    }
}
