using Inventory.Application.DTOs;

namespace Inventory.Application.Abstractions;

public interface IGetStockHandler
{
    Task<IReadOnlyCollection<StockItemDto>> HandleAsync(
        CancellationToken cancellationToken = default);
}