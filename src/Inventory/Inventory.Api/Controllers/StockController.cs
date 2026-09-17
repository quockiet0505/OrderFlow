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
        if (string.IsNullOrWhiteSpace(sku))
        {
            return BadRequest(new { message = "SKU path parameter cannot be empty." });
        }

        if (request == null)
        {
            return BadRequest(new { message = "Request body cannot be null." });
        }

        if (request.Quantity == 0)
        {
            return BadRequest(new { message = "Adjustment quantity cannot be zero." });
        }

        var result = await _stockService.AdjustStockAsync(sku.Trim(), request.Quantity);
        if (result == null)
        {
            return NotFound(new { message = $"SKU '{sku}' not found." });
        }

        return Ok(result);
    }
}
