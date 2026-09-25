using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;

namespace Orders.Application.Handlers;

public class GetOrdersByCustomerHandler : IGetOrdersByCustomerHandler
{
    private readonly IOrdersDbContext _dbContext;

    public GetOrdersByCustomerHandler(IOrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<OrderSummaryResponse>> HandleAsync(string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId)) return [];

        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId.Trim())
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(order => new OrderSummaryResponse(
            order.Id,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt
        )).ToList();
    }
}
