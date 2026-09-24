using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Application.Abstractions;
using Inventory.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StockController : ControllerBase
{
    private readonly IGetStockHandler _getStockHandler;
    private readonly IAdjustStockHandler _adjustStockHandler;

    public StockController(
        IGetStockHandler getStockHandler,
        IAdjustStockHandler adjustStockHandler)
    {
        _getStockHandler = getStockHandler;
        _adjustStockHandler = adjustStockHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<StockItemDto>>> GetStock(
        CancellationToken cancellationToken)
    {
        var stock = await _getStockHandler.HandleAsync(cancellationToken);

        return Ok(stock);
    }

    [HttpPost("{sku}/adjust")]
    public async Task<ActionResult<StockItemDto>> AdjustStock(
        string sku,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return BadRequest(new { message = "SKU path parameter cannot be empty." });
        }

        var result = await _adjustStockHandler.HandleAsync(
            sku.Trim(),
            request.Quantity,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = $"SKU '{sku}' not found." });
        }

        return Ok(result);
    }
}