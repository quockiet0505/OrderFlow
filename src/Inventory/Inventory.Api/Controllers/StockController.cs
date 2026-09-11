using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Application.DTOs;
using Inventory.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    // Get stock items
    [HttpGet]
    public async Task<ActionResult<List<StockItemDto>>> GetStock()
    {
        var stock = await _stockService.GetStockAsync();
        return Ok(stock);
    }

    // Adjust stock quantity
    [HttpPost("{sku}/adjust")]
    public async Task<ActionResult<StockItemDto>> AdjustStock(string sku, [FromBody] AdjustStockRequest request)
    {
        var result = await _stockService.AdjustStockAsync(sku, request.Quantity);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
