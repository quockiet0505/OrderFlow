using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Application.Handlers;
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
        var handler = scope.ServiceProvider.GetRequiredService<IInventoryCommandHandler>();

        using var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        
        if (!root.TryGetProperty("EventId", out var eventIdProp) || !Guid.TryParse(eventIdProp.GetString(), out var eventId)) return;

        var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == eventId, cancellationToken);
        if (exists) return;

        dbContext.InboxMessages.Add(new InboxMessage { EventId = eventId, ProcessedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);

        if (root.TryGetProperty("Reason", out _))
        {
            var @event = JsonSerializer.Deserialize<PaymentFailedEvent>(messageJson);
            if (@event != null)
            {
                await handler.HandleAsync(@event, cancellationToken);
            }
        }
        else
        {
            var @event = JsonSerializer.Deserialize<PaymentSucceededEvent>(messageJson);
            if (@event != null)
            {
                await handler.HandleAsync(@event, cancellationToken);
            }
        }

        Logger.LogInformation("Inventory consumer processed payment event EventId: {EventId}", eventId);
    }
}
