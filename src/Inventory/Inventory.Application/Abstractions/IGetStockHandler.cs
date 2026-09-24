using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Application.DTOs;

namespace Inventory.Application.Abstractions;

public interface IGetStockHandler
{
    Task<List<StockItemDto>> HandleAsync(
        CancellationToken cancellationToken = default);
}