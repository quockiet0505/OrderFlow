namespace Orders.Application.DTOs;

public record CreateOrderResponse(
    Guid OrderId,
    Guid CorrelationId,
    string Status
);