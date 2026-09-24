using System.Text.Json;
using OrderFlow.Contracts.Constants;
using OrderFlow.Contracts.DTOs;
using OrderFlow.Contracts.Events;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Handlers;

public class CreateOrderHandler : ICreateOrderHandler
{
    private readonly IOrdersDbContext _dbContext;

    public CreateOrderHandler(IOrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateOrderResponse> HandleAsync(
        CreateOrderApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var orderId = Guid.NewGuid();

        var customerId = request.CustomerId.Trim();

        var totalAmount = request.Lines
            .Sum(x => x.Quantity * x.UnitPrice);

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,

            Lines = request.Lines
                .Select(line => new OrderLine
                {
                    OrderId = orderId,
                    Sku = line.Sku.Trim(),
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice
                })
                .ToList(),

            SagaState = new OrderSagaState
            {
                OrderId = orderId,
                ReservationCompleted = false,
                PaymentCompleted = false
            }
        };

        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            request.Lines
                .Select(x => new OrderLineItemDto(
                    x.Sku.Trim(),
                    x.Quantity,
                    x.UnitPrice))
                .ToList(),
            totalAmount
        );

        var outboxMessage = new OutboxMessage
        {
            EventId = orderPlacedEvent.EventId,
            Topic = PulsarTopics.OrderPlaced,
            Payload = JsonSerializer.Serialize(orderPlacedEvent),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        _dbContext.OutboxMessages.Add(outboxMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResponse(
            order.Id,
            order.Id,
            order.Status.ToString());
    }
}