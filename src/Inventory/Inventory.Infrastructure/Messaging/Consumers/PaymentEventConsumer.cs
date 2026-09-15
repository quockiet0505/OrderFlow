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

namespace Inventory.Infrastructure.BackgroundServices;

public class PaymentEventConsumerService : PulsarConsumerBase
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentEventConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<PaymentEventConsumerService> logger)
        : base(
            configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650",
            "persistent://public/default/payment-events",
            "inventory-payment-sub",
            logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ConsumeMessageAsync(string topic, string messageJson, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        
        if (!root.TryGetProperty("EventId", out var eventIdProp) || !Guid.TryParse(eventIdProp.GetString(), out var eventId)) return;
        if (!root.TryGetProperty("OrderId", out var orderIdProp) || !Guid.TryParse(orderIdProp.GetString(), out var orderId)) return;

        var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == eventId, cancellationToken);
        if (exists) return;

        dbContext.InboxMessages.Add(new InboxMessage { EventId = eventId });

        var isFailed = root.TryGetProperty("Reason", out _);
        
        var reservations = await dbContext.Reservations
            .Where(x => x.OrderId == orderId && x.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            var item = await dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == reservation.Sku, cancellationToken);
            if (item != null)
            {
                if (isFailed) // Compensation
                {
                    reservation.Status = ReservationStatus.Released;
                    item.QuantityReserved -= reservation.Quantity;
                }
                else // Succeeded
                {
                    reservation.Status = ReservationStatus.Consumed;
                    item.QuantityReserved -= reservation.Quantity;
                    item.QuantityOnHand -= reservation.Quantity; // Permanently consume stock
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Inventory processed payment event for OrderId: {OrderId}. Failed: {IsFailed}", orderId, isFailed);
    }
}
