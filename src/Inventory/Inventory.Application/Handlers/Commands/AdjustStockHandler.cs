using Inventory.Application.DTOs;
using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Handlers.Commands;

public class AdjustStockHandler : IAdjustStockHandler
{
    private readonly IInventoryDbContext _dbContext;

    public AdjustStockHandler(IInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockItemDto?> HandleAsync(string sku, int quantity, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == sku, cancellationToken);
        if (item is null)
        {
            item = new StockItem
            {
                Sku = sku,
                QuantityOnHand = quantity,
                QuantityReserved = 0
            };
            _dbContext.StockItems.Add(item);
        }
        else
        {
            item.QuantityOnHand += quantity;
            if (item.QuantityOnHand < 0) item.QuantityOnHand = 0;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new StockItemDto(
            item.Sku,
            item.QuantityOnHand,
            item.QuantityReserved,
            item.Available
        );
    }
}
