using System;

namespace Orders.Domain.Entities;

public class OrderSagaState
{
    public Guid OrderId { get; set; }
    public bool ReservationCompleted { get; set; }
    public bool PaymentCompleted { get; set; }
    public Guid? LastProcessedEventId { get; set; }

    public Order? Order { get; set; }
}
