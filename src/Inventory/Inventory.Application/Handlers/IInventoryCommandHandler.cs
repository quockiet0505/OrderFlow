using OrderFlow.Contracts.Events;

namespace Inventory.Application.Handlers;

public interface IInventoryCommandHandler :
    IIntegrationEventHandler<OrderPlacedEvent>,
    IIntegrationEventHandler<PaymentSucceededEvent>,
    IIntegrationEventHandler<PaymentFailedEvent>
{
}
