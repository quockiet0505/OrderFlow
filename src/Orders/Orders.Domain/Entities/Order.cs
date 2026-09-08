using System;
using System.Collections.Generic;
using Orders.Domain.Enums;

namespace Orders.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderLine> Lines { get; set; } = new();
    public OrderSagaState? SagaState { get; set; }
}
