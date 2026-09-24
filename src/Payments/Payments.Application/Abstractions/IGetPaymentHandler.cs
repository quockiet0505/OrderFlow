using Payments.Application.DTOs;

namespace Payments.Application.Abstractions;

public interface IGetPaymentHandler
{
    Task<PaymentResponse?> HandleAsync(
        Guid orderId, 
        CancellationToken cancellationToken = default);
}
