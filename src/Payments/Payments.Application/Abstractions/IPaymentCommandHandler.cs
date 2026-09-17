using OrderFlow.Contracts.Events;

namespace Payments.Application.Abstractions;

public interface IPaymentCommandHandler :
    IIntegrationEventHandler<ReservationSucceededEvent>
{
}
