using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Application.DTOs;

namespace Payments.Application.Handlers;

public class GetPaymentHandler : IGetPaymentHandler
{
    private readonly IPaymentsDbContext _dbContext;

    public GetPaymentHandler(IPaymentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentResponse?> HandleAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty) return null;

        var payment = await _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);

        if (payment == null) return null;

        return new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Status.ToString(),
            payment.CreatedAt
        );
    }
}
