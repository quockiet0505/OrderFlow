namespace Orders.Application.DTOs;

public record OrderSummaryResponse(
    Guid Id,
    string Status,
    decimal Total,
    DateTime CreatedAt
);

