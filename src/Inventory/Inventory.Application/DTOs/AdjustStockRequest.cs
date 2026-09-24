namespace Inventory.Application.DTOs;

public record AdjustStockRequest
{

    [NotZero(ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; init; }
}

