using OrderFlow.Contracts.Events;

namespace Orders.Application.Handlers;

public interface IOrderSagaHandler :
    IIntegrationEventHandler<ReservationSucceededEvent>,
    IIntegrationEventHandler<ReservationFailedEvent>,
    IIntegrationEventHandler<PaymentSucceededEvent>,
    IIntegrationEventHandler<PaymentFailedEvent>
{
}
