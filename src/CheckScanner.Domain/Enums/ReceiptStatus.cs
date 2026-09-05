namespace CheckScanner.Domain.Enums;

/// <summary>
/// Where a receipt sits in the parse pipeline. Only <see cref="Uploaded"/> is
/// produced today -- <see cref="Parsed"/> and <see cref="ParseFailed"/> are
/// where a future IReceiptParser implementation will land.
/// </summary>
public enum ReceiptStatus
{
    Uploaded,
    Parsed,
    ParseFailed
}
