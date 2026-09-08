using System;
using System.Collections.Generic;
using OrderFlow.Contracts.DTOs;

namespace OrderFlow.Contracts.Events;

public record ReservationSucceededEvent : IntegrationEvent
{
    public string ReservationId { get; init; } = string.Empty;
    public List<ReservedLineDto> Lines { get; init; } = new();

    public ReservationSucceededEvent() { }

    public ReservationSucceededEvent(Guid orderId, string reservationId, List<ReservedLineDto> lines)
        : base(orderId)
    {
        ReservationId = reservationId;
        Lines = lines;
    }
}
