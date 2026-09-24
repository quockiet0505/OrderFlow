using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;

namespace Orders.Application.Handlers;

public class GetOrderHandler : IGetOrderHandler
{
    private readonly IOrdersDbContext _dbContext;

    public GetOrderHandler(IOrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderResponse?> HandleAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty) return null;

        var order = await _dbContext.Orders
            .Include(x => x.Lines)
            .Include(x => x.SagaState)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

        if (order == null) return null;

        return new OrderResponse(
            order.Id,
            order.Id,
            order.Status.ToString(),
            order.TotalAmount,
            order.SagaState?.ReservationCompleted ?? false,
            order.SagaState?.PaymentCompleted ?? false,
            order.Lines
                .Select(l => new OrderLineResponse(l.Sku, l.Quantity, l.UnitPrice))
                .ToList(),
            order.CreatedAt,
            order.UpdatedAt
        );
    }
}
