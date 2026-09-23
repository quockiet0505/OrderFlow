using System.Threading;
using System.Threading.Tasks;

namespace OrderFlow.Contracts.Events;

public interface IIntegrationEventHandler< Event> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
