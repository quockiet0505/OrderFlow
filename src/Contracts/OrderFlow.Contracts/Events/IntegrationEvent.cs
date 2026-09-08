using System;

namespace OrderFlow.Contracts.Events;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid OrderId { get; init; }
    public Guid CorrelationId => OrderId;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    protected IntegrationEvent() { }

    protected IntegrationEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
