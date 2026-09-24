using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar.Abstractions;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Constants;
using OrderFlow.Contracts.Events;
using Shared.Infrastructure.Messaging;

namespace Inventory.Infrastructure.Messaging.Consumers;

public class OrderEventConsumerService : PulsarConsumerBase
{
    private readonly IServiceProvider _serviceProvider;

    public OrderEventConsumerService(
        IServiceProvider serviceProvider,
        IPulsarClient pulsarClient,
        ILogger<OrderEventConsumerService> logger)
        : base(
            pulsarClient,
            PulsarTopics.OrderPlaced,
            PulsarSubscriptions.InventoryOrder,
            logger)
    {
        _serviceProvider = serviceProvider;
    }

    private static async Task ProcessEventAsync(
        InventoryDbContext dbContext,
        Guid eventId,
        Func<Task> handleEvent,
        CancellationToken cancellationToken)
    {
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
        var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<OrderPlacedEvent>>();

        var orderEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(
            messageJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (orderEvent is null || orderEvent.EventId == Guid.Empty) return;

        await ProcessEventAsync(
            dbContext,
            orderEvent.EventId,
            () => handler.HandleAsync(orderEvent, cancellationToken),
            cancellationToken
        );

        Logger.LogInformation("Inventory consumer processed order event ");
    }
}
