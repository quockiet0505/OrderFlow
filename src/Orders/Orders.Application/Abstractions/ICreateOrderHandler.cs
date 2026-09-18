using Orders.Application.DTOs;

namespace Orders.Application.Abstractions;
public interface ICreateOrderHandler
{
    Task<CreateOrderResponse> HandlerAsync(
        CreateOrderApiRequest request, 
        CancellationToken cancellationToken
    );
}