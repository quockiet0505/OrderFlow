using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Application.Handlers;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Outbox;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.DTOs;
using OrderFlow.Contracts.Events;

namespace Inventory.Infrastructure.Handlers;

public class InventoryCommandHandler : IInventoryCommandHandler
{
    private readonly InventoryDbContext _dbContext;
    private readonly ILogger<InventoryCommandHandler> _logger;

    public InventoryCommandHandler(InventoryDbContext dbContext, ILogger<InventoryCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken cancellationToken = default)
    {
        bool isSuccess = true;
        string failureReason = string.Empty;

        // Check stock availability
        foreach (var line in @event.Lines)
        {
            var item = await _dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == line.Sku, cancellationToken);
            if (item == null || item.Available < line.Quantity)
            {
                isSuccess = false;
                failureReason = $"Not enough stock for SKU: {line.Sku}";
                break;
            }
        }

        IntegrationEvent outEvent;

        if (isSuccess)
        {
            foreach (var line in @event.Lines)
            {
                var item = await _dbContext.StockItems.FirstAsync(x => x.Sku == line.Sku, cancellationToken);
                item.QuantityReserved += line.Quantity;

                _dbContext.Reservations.Add(new Reservation
                {
                    OrderId = @event.OrderId,
                    Sku = line.Sku,
                    Quantity = line.Quantity,
                    Status = ReservationStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var reservedLines = @event.Lines
                .Select(l => new ReservedLineDto(l.Sku, l.Quantity, l.UnitPrice))
                .ToList();

            outEvent = new ReservationSucceededEvent(@event.OrderId, Guid.NewGuid().ToString(), reservedLines);
        }
        else
        {
            outEvent = new ReservationFailedEvent(@event.OrderId, failureReason);
        }

        var outboxMessage = new OutboxMessage
        {
            EventId = outEvent.EventId,
            Topic = "persistent://public/default/reservation-events",
            Payload = JsonSerializer.Serialize((object)outEvent),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory handled OrderPlacedEvent for OrderId: {OrderId}, Success: {Success}", @event.OrderId, isSuccess);
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken cancellationToken = default)
    {
        var reservations = await _dbContext.Reservations
            .Where(x => x.OrderId == @event.OrderId && x.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            var item = await _dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == reservation.Sku, cancellationToken);
            if (item != null)
            {
                reservation.Status = ReservationStatus.Consumed;
                item.QuantityReserved -= reservation.Quantity;
                item.QuantityOnHand -= reservation.Quantity; // Permanently consume stock
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inventory consumed stock for OrderId: {OrderId}", @event.OrderId);
    }

    public async Task HandleAsync(PaymentFailedEvent @event, CancellationToken cancellationToken = default)
    {
        // Compensation logic: Release reserved stock
        var reservations = await _dbContext.Reservations
            .Where(x => x.OrderId == @event.OrderId && x.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            var item = await _dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == reservation.Sku, cancellationToken);
            if (item != null)
            {
                reservation.Status = ReservationStatus.Released;
                item.QuantityReserved -= reservation.Quantity; // Restore available stock
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inventory compensated stock (released) for OrderId: {OrderId}", @event.OrderId);
    }
}
