using Orders.Application.DTOs;

namespace Orders.Application.Abstractions;

public interface IGetOrderHandler
{
    Task<OrderResponse?> HandleAsync(
        Guid orderId, 
        CancellationToken cancellationToken = default);
}
