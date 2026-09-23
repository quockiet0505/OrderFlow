using System.Threading;
using System.Threading.Tasks;
using Orders.Application.DTOs;

namespace Orders.Application.Abstractions;

public interface ICreateOrderHandler
{
    Task<CreateOrderResponse> HandleAsync(
        CreateOrderApiRequest request, 
        CancellationToken cancellationToken = default);
}