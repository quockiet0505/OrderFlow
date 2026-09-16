using System.Threading;
using System.Threading.Tasks;

namespace OrderFlow.Contracts.Events;

public interface IIntegrationEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
