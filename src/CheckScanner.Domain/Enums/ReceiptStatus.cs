namespace CheckScanner.Domain.Enums;

/// <summary>Where a receipt sits in the parse pipeline.</summary>
public enum ReceiptStatus
{
    Uploaded,
    Parsed,
    /// <summary>Parsed successfully, but line items didn't reconcile with the printed total.</summary>
    Flagged,
    ParseFailed
}
