using Orders.Application.DTOs;

namespace Orders.Application.Abstractions;

public interface IGetOrdersByCustomerHandler
{
    Task<IReadOnlyCollection<OrderSummaryResponse>> HandleAsync(
        string customerId, 
        CancellationToken cancellationToken = default);
}
