using System;
using System.Collections.Generic;
using OrderFlow.Contracts.DTOs;

namespace OrderFlow.Contracts.Events;

public record StockReleasedEvent : IntegrationEvent
{
    public List<ReservedLineDto> ReleasedLines { get; init; } = new();

    public StockReleasedEvent() { }

    public StockReleasedEvent(Guid orderId, List<ReservedLineDto> releasedLines)
        : base(orderId)
    {
        ReleasedLines = releasedLines;
    }
}
