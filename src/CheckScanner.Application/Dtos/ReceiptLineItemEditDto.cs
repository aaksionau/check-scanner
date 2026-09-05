namespace CheckScanner.Application.Dtos;

/// <summary>
/// Mutable line-item shape for the line-item editor's two-way data binding --
/// <see cref="Domain.Entities.ReceiptLineItem"/>'s init-only properties can't
/// be reassigned by Blazor's @bind after construction.
/// </summary>
public sealed class ReceiptLineItemEditDto
{
    /// <summary>Null for a line the user added in the editor rather than one the parser produced.</summary>
    public Guid? Id { get; set; }
    public string RawText { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
