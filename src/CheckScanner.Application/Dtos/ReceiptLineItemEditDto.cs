using CheckScanner.Domain.Entities;

namespace CheckScanner.Application.Dtos;

/// <summary>
/// Mutable line-item shape for the line-item editor's two-way data binding --
/// <see cref="ReceiptLineItem"/>'s init-only properties can't be reassigned by
/// Blazor's @bind after construction.
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

    public static ReceiptLineItemEditDto FromEntity(ReceiptLineItem lineItem) =>
        new()
        {
            Id = lineItem.Id,
            RawText = lineItem.RawText,
            Description = lineItem.Description,
            Category = lineItem.Category,
            Quantity = lineItem.Quantity,
            UnitPrice = lineItem.UnitPrice,
            LineTotal = lineItem.LineTotal
        };

    public ReceiptLineItem ToEntity() =>
        new()
        {
            Id = Id ?? Guid.NewGuid(),
            RawText = RawText,
            Description = Description,
            Category = Category,
            Quantity = Quantity,
            UnitPrice = UnitPrice,
            LineTotal = LineTotal
        };
}
