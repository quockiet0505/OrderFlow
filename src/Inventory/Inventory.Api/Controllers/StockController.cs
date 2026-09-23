using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Application.Abstractions;
using Inventory.Application.DTOs;
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

    [HttpGet]
    public async Task<ActionResult<List<StockItemDto>>> GetStock()
    {
        var stock = await _stockService.GetStockAsync();
        return Ok(stock);
    }

    [HttpPost("{sku}/adjust")]
    public async Task<ActionResult<StockItemDto>> AdjustStock(
        string sku, 
        [FromBody] AdjustStockRequest request)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return BadRequest(new { message = "SKU path parameter cannot be empty." });
        }

        var result = await _stockService.AdjustStockAsync(sku.Trim(), request.Quantity);
        if (result is null)
        {
            return NotFound(new { message = $"SKU '{sku}' not found." });
        }

        return Ok(result);
    }
}
