namespace Inventory.Application.DTOs;

public record StockItemDto(
    string Sku,
    int QuantityOnHand,
    int QuantityReserved,
    int Available
);

public record AdjustStockRequest(
    int Quantity
);
