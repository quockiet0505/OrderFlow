using System.Text.Json;
using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Constants;
using OrderFlow.Contracts.DTOs;
using OrderFlow.Contracts.Events;

namespace Inventory.Application.Handlers;

public class OrderPlacedHandler : IIntegrationEventHandler<OrderPlacedEvent>
{
    private readonly IInventoryDbContext _dbContext;
    private readonly ILogger<OrderPlacedHandler> _logger;

    public OrderPlacedHandler(IInventoryDbContext dbContext, ILogger<OrderPlacedHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event is null || @event.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Received null or empty OrderPlacedEvent payload.");
            return;
        }

        if (@event.Lines is null || @event.Lines.Count == 0)
        {
            _logger.LogWarning("OrderPlacedEvent contains no order lines.");
            var emptyFailureEvent = new ReservationFailedEvent(@event.OrderId, "Order contains no items.");
            await SaveOutboxMessageAsync(emptyFailureEvent, cancellationToken);
            return;
        }

        foreach (var line in @event.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.Sku) || line.Quantity <= 0 || line.UnitPrice < 0)
            {
                _logger.LogWarning("OrderPlacedEvent contains invalid line item");
                var invalidLineFailureEvent = new ReservationFailedEvent(@event.OrderId, $"Invalid order line item payload for SKU: {line.Sku}");
                await SaveOutboxMessageAsync(invalidLineFailureEvent, cancellationToken);
                return;
            }
        }

        var existingReservations = await _dbContext.Reservations
            .Where(x => x.OrderId == @event.OrderId)
            .ToListAsync(cancellationToken);

        if (existingReservations.Count > 0)
        {
            _logger.LogInformation("Reservations for OrderId  already exist.");
            return;
        }

        var aggregatedLines = @event.Lines
            .GroupBy(l => l.Sku)
            .Select(g => new
            {
                Sku = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity),
                UnitPrice = g.First().UnitPrice
            })
            .ToList();

        bool isSuccess = true;
        string failureReason = string.Empty;

        foreach (var itemDemand in aggregatedLines)
        {
            var item = await _dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == itemDemand.Sku, cancellationToken);
            if (item is null)
            {
                isSuccess = false;
                failureReason = $"SKU not found in stock catalog: {itemDemand.Sku}";
                break;
            }

            if (item.Available < itemDemand.TotalQuantity)
            {
                isSuccess = false;
                failureReason = $"Insufficient available stock for SKU: {itemDemand.Sku}. Requested: {itemDemand.TotalQuantity}, Available: {item.Available}";
                break;
            }
        }

        IntegrationEvent outEvent;

        if (isSuccess)
        {
            foreach (var itemDemand in aggregatedLines)
            {
                var item = await _dbContext.StockItems.FirstAsync(x => x.Sku == itemDemand.Sku, cancellationToken);
                item.QuantityReserved += itemDemand.TotalQuantity;

                _dbContext.Reservations.Add(new Reservation
                {
                    OrderId = @event.OrderId,
                    Sku = itemDemand.Sku,
                    Quantity = itemDemand.TotalQuantity,
                    Status = ReservationStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var reservedLines = aggregatedLines
                .Select(l => new ReservedLineDto(l.Sku, l.TotalQuantity, l.UnitPrice))
                .ToList();

            outEvent = new ReservationSucceededEvent(@event.OrderId, Guid.NewGuid().ToString(), reservedLines);
        }
        else
        {
            outEvent = new ReservationFailedEvent(@event.OrderId, failureReason);
        }

        await SaveOutboxMessageAsync(outEvent, cancellationToken);
        _logger.LogInformation("Inventory handled OrderPlacedEvent");
    }

    private async Task SaveOutboxMessageAsync(IntegrationEvent outEvent, CancellationToken cancellationToken)
    {
        var outboxMessage = new OutboxMessage
        {
            EventId = outEvent.EventId,
            Topic = PulsarTopics.ReservationEvents,
            Payload = JsonSerializer.Serialize((object)outEvent),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
