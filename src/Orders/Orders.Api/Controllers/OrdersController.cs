using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OrderFlow.Contracts.DTOs;
using OrderFlow.Contracts.Events;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Infrastructure.Outbox;
using Orders.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Orders.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _dbContext;

    public OrdersController(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // POST /orders
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderApiRequest request)
    {
        if (request == null || request.Lines == null || !request.Lines.Any())
        {
            return BadRequest(new { message = "Order must contain at least one line item." });
        }

        var orderId = Guid.NewGuid();
        decimal totalAmount = request.Lines.Sum(x => x.Quantity * x.UnitPrice);

        var order = new Order
        {
            Id = orderId,
            CustomerId = request.CustomerId,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Lines = request.Lines.Select(l => new OrderLine
            {
                OrderId = orderId,
                Sku = l.Sku,
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
            request.CustomerId,
            request.Lines.Select(x => new OrderLineItemDto(x.Sku, x.Quantity, x.UnitPrice)).ToList(),
            totalAmount
        );

        var outboxMessage = new OutboxMessage
        {
            EventId = orderPlacedEvent.EventId,
            Topic = "persistent://public/default/orders.order-placed",
            Payload = JsonSerializer.Serialize(orderPlacedEvent),
            CreatedAt = DateTime.UtcNow
        };

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            _dbContext.Orders.Add(order);
            _dbContext.OutboxMessages.Add(outboxMessage);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return Accepted(new
        {
            orderId = order.Id,
            correlationId = order.Id,
            status = order.Status.ToString()
        });
    }

    // GET /orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _dbContext.Orders
            .Include(x => x.Lines)
            .Include(x => x.SagaState)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();

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

    // GET /orders?customerId={customerId}
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
            .Where(x => x.CustomerId == customerId)
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

