using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory.Application.DTOs;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Services;

public class StockService : IStockService
{
    private readonly InventoryDbContext _dbContext;

    public StockService(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<StockItemDto>> GetStockAsync()
    {
        var items = await _dbContext.StockItems.AsNoTracking().ToListAsync();
        return items.Select(x => new StockItemDto(
            x.Sku,
            x.QuantityOnHand,
            x.QuantityReserved,
            x.Available
        )).ToList();
    }

    public async Task<StockItemDto?> AdjustStockAsync(string sku, int quantity)
    {
        var item = await _dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == sku);
        if (item == null)
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

        await _dbContext.SaveChangesAsync();

        return new StockItemDto(
            item.Sku,
            item.QuantityOnHand,
            item.QuantityReserved,
            item.Available
        );
    }
}
