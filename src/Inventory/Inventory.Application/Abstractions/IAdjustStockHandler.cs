using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Application.DTOs;

namespace Inventory.Application.Abstractions;

public interface IAdjustStockHandler
{
    Task<List<StockItemDto>> HandleAsync(
        CancellationToken cancellationToken = default);
}