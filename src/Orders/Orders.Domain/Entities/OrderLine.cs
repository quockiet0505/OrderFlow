using System;

namespace Orders.Domain.Entities;

public class OrderLine
{
    public long Id { get; set; }
    public Guid OrderId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Order? Order { get; set; }
}
