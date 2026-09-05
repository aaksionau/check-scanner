namespace CheckScanner.Web.Components.Shared;

/// <summary>Shared fallback text for the store-name/purchase-date pair shown on every receipt list/detail view.</summary>
public static class ReceiptDisplayFormat
{
    public static string StoreName(string? storeName) => storeName ?? "Unknown store";

    public static string PurchasedAt(DateTimeOffset? purchasedAt) => purchasedAt?.ToString("MMM d, yyyy") ?? "Date pending";
}
