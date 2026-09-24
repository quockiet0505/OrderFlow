namespace Payments.Application.DTOs;

public record PaymentResponse(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Status,
    DateTime CreatedAt
);
