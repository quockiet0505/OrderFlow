using System;

namespace OrderFlow.Contracts.Events;

public record PaymentSucceededEvent : IntegrationEvent
{
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }

    public PaymentSucceededEvent() { }

    public PaymentSucceededEvent(Guid orderId, Guid paymentId, decimal amount)
        : base(orderId)
    {
        PaymentId = paymentId;
        Amount = amount;
    }
}
