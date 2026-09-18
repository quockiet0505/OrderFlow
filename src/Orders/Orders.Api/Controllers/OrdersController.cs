using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts.DTOs;
using OrderFlow.Contracts.Events;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersDbContext _dbContext;

    public OrdersController(IOrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Create new order
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderApiRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body cannot be null." });
        }

        if (string.IsNullOrWhiteSpace(request.CustomerId))
        {
            return BadRequest(new { message = "CustomerId is required and cannot be empty." });
        }

        if (request.Lines == null || !request.Lines.Any())
        {
            return BadRequest(new { message = "Order must contain at least one line item." });
        }

        foreach (var line in request.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.Sku))
            {
                return BadRequest(new { message = "SKU is required for all order lines." });
            }

            if (line.Quantity <= 0)
            {
                return BadRequest(new { message = $"Quantity for SKU '{line.Sku}' must be greater than zero." });
            }

            if (line.UnitPrice < 0)
            {
                return BadRequest(new { message = $"UnitPrice for SKU '{line.Sku}' cannot be negative." });
            }
        }

        var orderId = Guid.NewGuid();
        decimal totalAmount = request.Lines.Sum(x => x.Quantity * x.UnitPrice);

        var order = new Order
        {
            Id = orderId,
            CustomerId = request.CustomerId.Trim(),
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Lines = request.Lines.Select(l => new OrderLine
            {
                OrderId = orderId,
                Sku = l.Sku.Trim(),
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList(),
            SagaState = new OrderSagaState
            {
                OrderId = orderId,
                ReservationCompleted = false,
                PaymentCompleted = false
            }
        };

        // Transactional Outbox Event: OrderPlaced
        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            request.CustomerId.Trim(),
            request.Lines.Select(x => new OrderLineItemDto(x.Sku.Trim(), x.Quantity, x.UnitPrice)).ToList(),
            totalAmount
        );

        var outboxMessage = new OutboxMessage
        {
            EventId = orderPlacedEvent.EventId,
            Topic = "persistent://public/default/orders.order-placed",
            Payload = JsonSerializer.Serialize(orderPlacedEvent),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        _dbContext.OutboxMessages.Add(outboxMessage);

        await _dbContext.SaveChangesAsync();

        return Accepted(new
        {
            orderId = order.Id,
            correlationId = order.Id,
            status = order.Status.ToString()
        });
    }

    // Get order by ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid Order ID." });
        }

        var order = await _dbContext.Orders
            .Include(x => x.Lines)
            .Include(x => x.SagaState)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound(new { message = $"Order {id} not found." });

        return Ok(new
        {
            orderId = order.Id,
            customerId = order.CustomerId,
            status = order.Status.ToString(),
            totalAmount = order.TotalAmount,
            reservationCompleted = order.SagaState?.ReservationCompleted ?? false,
            paymentCompleted = order.SagaState?.PaymentCompleted ?? false,
            lines = order.Lines.Select(l => new
            {
                sku = l.Sku,
                quantity = l.Quantity,
                unitPrice = l.UnitPrice
            }),
            createdAt = order.CreatedAt,
            updatedAt = order.UpdatedAt
        });
    }

    // Get orders by customer ID
    [HttpGet]
    public async Task<IActionResult> GetOrdersByCustomer([FromQuery] string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return BadRequest(new { message = "CustomerId is required." });
        }

        var orders = await _dbContext.Orders
            .Include(x => x.Lines)
            .Include(x => x.SagaState)
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId.Trim())
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var response = orders.Select(order => new
        {
            id = order.Id,
            status = order.Status.ToString(),
            total = order.TotalAmount,
            createdAt = order.CreatedAt
        });

        return Ok(response);
    }
}

// Orders.Api
// └── Controllers
//     └── OrdersController
//           │
//           ├── HTTP Request
//           ↓
// Orders.Application
// ├── Abstractions
// │   ├── ICreateOrderHandler
// │   ├── IGetOrderHandler
// │   └── IGetOrdersByCustomerHandler
// │
// ├── DTOs
// │   ├── CreateOrderRequest
// │   ├── CreateOrderResponse
// │   ├── OrderResponse
// │   └── OrderSummaryResponse
// │
// └── Handlers
//     ├── CreateOrderHandler
//     ├── GetOrderHandler
//     └── GetOrdersByCustomerHandler
