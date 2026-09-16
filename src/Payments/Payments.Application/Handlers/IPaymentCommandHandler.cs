using OrderFlow.Contracts.Events;

namespace Payments.Application.Handlers;

public interface IPaymentCommandHandler :
    IIntegrationEventHandler<ReservationSucceededEvent>
{
}
