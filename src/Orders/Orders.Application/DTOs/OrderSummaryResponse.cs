namespace Orders.Application.DTOs;

public record OrderSummaryResponse(
    Guid OrderId,
    string Status,
    decimal TotalAmount,
    DateTime DateTime
);
