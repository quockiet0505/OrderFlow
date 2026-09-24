using System;
using System.Collections.Generic;
using OrderFlow.Contracts.DTOs;

namespace OrderFlow.Contracts.Events;

public record ReservationSucceededEvent : IntegrationEvent
{
    public string ReservationId { get; init; } = string.Empty;

    public IReadOnlyCollection<ReservedLineDto> Lines { get; init; } = [];

    public ReservationSucceededEvent() { }

    public ReservationSucceededEvent(
        Guid orderId, 
        string reservationId, 
        IReadOnlyCollection<ReservedLineDto> lines)
        : base(orderId)
    {
        ReservationId = reservationId;
        Lines = lines;
    }
}
