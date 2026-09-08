using System;

namespace OrderFlow.Contracts.Events;

public record ReservationFailedEvent : IntegrationEvent
{
    public string Reason { get; init; } = string.Empty;

    public ReservationFailedEvent() { }

    public ReservationFailedEvent(Guid orderId, string reason)
        : base(orderId)
    {
        Reason = reason;
    }
}
