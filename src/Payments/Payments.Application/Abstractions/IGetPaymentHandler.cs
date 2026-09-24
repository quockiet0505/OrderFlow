using System;
using System.Threading;
using System.Threading.Tasks;
using Payments.Application.DTOs;

namespace Payments.Application.Abstractions;

public interface IGetPaymentHandler
{
    Task<PaymentResponse?> HandleAsync(
        Guid orderId, 
        CancellationToken cancellationToken = default);
}
