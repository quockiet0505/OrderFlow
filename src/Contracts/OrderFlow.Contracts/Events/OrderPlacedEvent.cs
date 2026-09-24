using OrderFlow.Contracts.DTOs;

namespace OrderFlow.Contracts.Events;

public record OrderPlacedEvent : IntegrationEvent
{
    public string CustomerId { get; init; } = string.Empty;
    public IReadOnlyCollection<OrderLineItemDto> Lines { get; init; } = [];
    public decimal TotalAmount { get; init; }

    public OrderPlacedEvent() { }

    public OrderPlacedEvent(
        Guid orderId, 
        string customerId, 
        IReadOnlyCollection<OrderLineItemDto> lines, 
        decimal totalAmount)
        : base(orderId)
    {
        CustomerId = customerId;
        Lines = lines;
        TotalAmount = totalAmount;
    }
}
