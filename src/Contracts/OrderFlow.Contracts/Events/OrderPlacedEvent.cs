using System;
using System.Collections.Generic;
using OrderFlow.Contracts.DTOs;

namespace OrderFlow.Contracts.Events;

public record OrderPlacedEvent : IntegrationEvent
{
    public string CustomerId { get; init; } = string.Empty;
    public List<OrderLineItemDto> Lines { get; init; } = new();
    public decimal TotalAmount { get; init; }

    public OrderPlacedEvent() { }

    public OrderPlacedEvent(Guid orderId, string customerId, List<OrderLineItemDto> lines, decimal totalAmount)
        : base(orderId)
    {
        CustomerId = customerId;
        Lines = lines;
        TotalAmount = totalAmount;
    }
}
