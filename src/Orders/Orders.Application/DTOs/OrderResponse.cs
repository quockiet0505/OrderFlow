namespace Orders.Application.DTOs;

public record OrderResponse(
    Guid OrderId,
    Guid CorrelationId,
    string Status,
    decimal TotalAmount,
    bool ReservationCompleted,
    bool PaymentCompleted,
    List<OrderLineResponse> Lines,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record OrderLineResponse(
    string Sku,
    int Quantity,
    decimal UnitPrice
);

