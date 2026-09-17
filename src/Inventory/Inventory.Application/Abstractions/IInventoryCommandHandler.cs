using OrderFlow.Contracts.Events;

namespace Inventory.Application.Abstractions;

public interface IInventoryCommandHandler :
    IIntegrationEventHandler<OrderPlacedEvent>,
    IIntegrationEventHandler<PaymentSucceededEvent>,
    IIntegrationEventHandler<PaymentFailedEvent>
{
}
