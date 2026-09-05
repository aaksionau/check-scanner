using CheckScanner.Domain.Enums;

namespace CheckScanner.Web.Components.Shared;

/// <summary>Shared fallback text and status styling shown on receipt list/detail views.</summary>
public static class ReceiptDisplayFormat
{
    public static string StoreName(string? storeName) => storeName ?? "Unknown store";

    public static string PurchasedAt(DateTimeOffset? purchasedAt) => purchasedAt?.ToString("MMM d, yyyy") ?? "Date pending";

    /// <summary>Card border/background classes for a Needs Review list row.</summary>
    public static string NeedsReviewCardClass(ReceiptStatus status) =>
        status == ReceiptStatus.ParseFailed ? "border-rose-200 bg-rose-50" : "border-amber-200 bg-amber-50";

    /// <summary>Badge shown next to the receipt title, or null when the receipt needs no callout.</summary>
    public static (string Text, string CssClass)? StatusBadge(ReceiptStatus status) => status switch
    {
        ReceiptStatus.Flagged => ("Needs review", "bg-amber-100 text-amber-800"),
        ReceiptStatus.ParseFailed => ("Parsing failed", "bg-rose-100 text-rose-800"),
        _ => null
    };
}
