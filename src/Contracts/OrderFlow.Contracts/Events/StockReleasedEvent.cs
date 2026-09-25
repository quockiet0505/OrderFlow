using OrderFlow.Contracts.DTOs;

namespace OrderFlow.Contracts.Events;

public record StockReleasedEvent : IntegrationEvent
{
    public IReadOnlyCollection<ReservedLineDto> ReleasedLines { get; init; } =[];

    public StockReleasedEvent() { }

    public StockReleasedEvent(Guid orderId, IReadOnlyCollection<ReservedLineDto> releasedLines)
        : base(orderId)
    {
        ReleasedLines = releasedLines;
    }
}
