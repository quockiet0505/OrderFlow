using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Application.DTOs;

namespace Inventory.Application.Services;

public interface IStockService
{
    Task<List<StockItemDto>> GetStockAsync();
    Task<StockItemDto?> AdjustStockAsync(string sku, int quantity);
}
