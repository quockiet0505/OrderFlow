using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;

namespace Orders.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ICreateOrderHandler _createOrderHandler;
    private readonly IGetOrderHandler _getOrderHandler;
    private readonly IGetOrdersByCustomerHandler _getOrdersByCustomerHandler;

    public OrdersController(
        ICreateOrderHandler createOrderHandler,
        IGetOrderHandler getOrderHandler,
        IGetOrdersByCustomerHandler getOrdersByCustomerHandler)
    {
        _createOrderHandler = createOrderHandler;
        _getOrderHandler = getOrderHandler;
        _getOrdersByCustomerHandler = getOrdersByCustomerHandler;
    }

    // Create new order
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderApiRequest request, CancellationToken cancellationToken)
    {
        var result = await _createOrderHandler.HandleAsync(request, cancellationToken);
        return Accepted(result);
    }

    // Get order by ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _getOrderHandler.HandleAsync(id, cancellationToken);
        if (order == null)
        {
            return NotFound(new { message = $"Order {id} not found." });
        }

        return Ok(order);
    }

    // Get orders by customer ID
    [HttpGet]
    public async Task<IActionResult> GetOrdersByCustomer([FromQuery] string customerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return BadRequest(new { message = "CustomerId is required." });
        }

        var orders = await _getOrdersByCustomerHandler.HandleAsync(customerId, cancellationToken);
        return Ok(orders);
    }
}

