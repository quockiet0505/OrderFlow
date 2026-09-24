using System.Threading;
using System.Threading.Tasks;
using Inventory.Application.DTOs;

namespace Inventory.Application.Abstractions;

public interface IAdjustStockHandler
{
    Task<StockItemDto?> HandleAsync(
        string sku,
        int quantity,
        CancellationToken cancellationToken = default);
}