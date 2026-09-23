using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orders.Application.DTOs;

namespace Orders.Application.Abstractions;

public interface IGetOrdersByCustomerHandler
{
    Task<List<OrderSummaryResponse>> HandleAsync(
        string customerId, 
        CancellationToken cancellationToken = default);
}
