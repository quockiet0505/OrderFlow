namespace Inventory.Domain.Entities;

public class StockItem
{
    public string Sku { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }

    public int Available => QuantityOnHand - QuantityReserved;
}
