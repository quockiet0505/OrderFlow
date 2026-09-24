using Inventory.Application.Abstractions;
using Inventory.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Handlers.Queries;

public class GetStockHandler : IGetStockHandler
{
    private readonly IInventoryDbContext _dbContext;

    public GetStockHandler(IInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<StockItemDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.StockItems
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return items.Select(x => new StockItemDto(
            x.Sku,
            x.QuantityOnHand,
            x.QuantityReserved,
            x.Available
        )).ToList();
    }
}