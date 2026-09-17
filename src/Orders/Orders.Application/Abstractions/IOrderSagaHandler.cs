using OrderFlow.Contracts.Events;

namespace Orders.Application.Abstractions;

public interface IOrderSagaHandler :
    IIntegrationEventHandler<ReservationSucceededEvent>,
    IIntegrationEventHandler<ReservationFailedEvent>,
    IIntegrationEventHandler<PaymentSucceededEvent>,
    IIntegrationEventHandler<PaymentFailedEvent>
{
}
