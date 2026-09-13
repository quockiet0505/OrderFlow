using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Inbox;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;
using Shared.Infrastructure.Messaging;
using LocalOutbox = Inventory.Infrastructure.Outbox.OutboxMessage;

namespace Inventory.Infrastructure.BackgroundServices;

public class OrderEventConsumerService : PulsarConsumerBase
{
    private readonly IServiceProvider _serviceProvider;

    public OrderEventConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OrderEventConsumerService> logger)
        : base(
            configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650",
            "persistent://public/default/order-events",
            "inventory-order-sub",
            logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ConsumeMessageAsync(string topic, string messageJson, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var orderEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(messageJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (orderEvent == null || orderEvent.EventId == Guid.Empty) return;

        var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == orderEvent.EventId, cancellationToken);
        if (exists) return;

        dbContext.InboxMessages.Add(new InboxMessage { EventId = orderEvent.EventId });

        bool isSuccess = true;
        string failureReason = "";

        // Attempt Reservation
        foreach (var line in orderEvent.Lines)
        {
            var item = await dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == line.Sku, cancellationToken);
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
            foreach (var line in orderEvent.Lines)
            {
                var item = await dbContext.StockItems.FirstAsync(x => x.Sku == line.Sku, cancellationToken);
                item.QuantityReserved += line.Quantity;
                
                dbContext.Reservations.Add(new Reservation
                {
                    OrderId = orderEvent.OrderId,
                    Sku = line.Sku,
                    Quantity = line.Quantity,
                    Status = ReservationStatus.Active
                });
            }
            outEvent = new ReservationSucceededEvent(orderEvent.OrderId);
        }
        else
        {
            outEvent = new ReservationFailedEvent(orderEvent.OrderId, failureReason);
        }

        dbContext.OutboxMessages.Add(new LocalOutbox
        {
            EventId = outEvent.EventId,
            Topic = "persistent://public/default/reservation-events",
            Payload = JsonSerializer.Serialize((object)outEvent),
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Inventory processed order event: {OrderId}, Success: {Success}", orderEvent.OrderId, isSuccess);
    }
}
