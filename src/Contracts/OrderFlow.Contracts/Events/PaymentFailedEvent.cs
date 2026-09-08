using System;

namespace OrderFlow.Contracts.Events;

public record PaymentFailedEvent : IntegrationEvent
{
    public string Reason { get; init; } = string.Empty;

    public PaymentFailedEvent() { }

    public PaymentFailedEvent(Guid orderId, string reason)
        : base(orderId)
    {
        Reason = reason;
    }
}
