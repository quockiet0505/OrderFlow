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

namespace Inventory.Infrastructure.Messaging.Consumers;

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

    private static async Task ProcessEventAsync(
        InventoryDbContext dbContext,
        Guid eventId,
        Func<Task> handleEvent,
        CancellationToken cancellationToken
    ){
        // create a transaction to ensure that the inbox message and the event
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var exists = await dbContext.InboxMessages
            .AnyAsync(x => x.EventId == eventId, cancellationToken);

        if (exists)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        await handleEvent();

        dbContext.InboxMessages.Add(new InboxMessage { EventId = eventId, ProcessedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
    

    protected override async Task ConsumeMessageAsync(string topic, string messageJson, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var handler = scope.ServiceProvider.GetRequiredService<IInventoryCommandHandler>();

        var orderEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(messageJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (orderEvent == null || orderEvent.EventId == Guid.Empty) return;

        var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == orderEvent.EventId, cancellationToken);
        if (exists) return;

        dbContext.InboxMessages.Add(new InboxMessage { EventId = orderEvent.EventId, ProcessedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);

        await handler.HandleAsync(orderEvent, cancellationToken);

        Logger.LogInformation("Inventory consumer processed order event: {OrderId}", orderEvent.OrderId);
    }
}
