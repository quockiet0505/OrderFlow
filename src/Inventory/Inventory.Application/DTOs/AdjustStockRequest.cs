namespace Inventory.Application.DTOs;

public record AdjustStockRequest
{
    public int Quantity { get; init; }
}

