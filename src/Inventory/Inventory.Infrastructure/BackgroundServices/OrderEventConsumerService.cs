using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Infrastructure.Inbox;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Messaging;

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

        using var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        
        if (root.TryGetProperty("EventId", out var eventIdProp) && Guid.TryParse(eventIdProp.GetString(), out var eventId))
        {
            var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == eventId, cancellationToken);
            if (exists) return;

            dbContext.InboxMessages.Add(new InboxMessage { EventId = eventId });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        Logger.LogInformation("Inventory processed order event: {Json}", messageJson);
    }
}
