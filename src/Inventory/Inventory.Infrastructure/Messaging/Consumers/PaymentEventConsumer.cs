using System.Text.Json;
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

public class PaymentEventConsumerService : PulsarConsumerBase
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentEventConsumerService(
        IServiceProvider serviceProvider,
        IPulsarClient pulsarClient,
        ILogger<PaymentEventConsumerService> logger)
        : base(
            pulsarClient,
            PulsarTopics.PaymentEvents,
            PulsarSubscriptions.InventoryPayment,
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

        using var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("EventId", out var eventIdProp) || !Guid.TryParse(eventIdProp.GetString(), out var eventId)) return;

        if (root.TryGetProperty("Reason", out _))
        {
            var @event = JsonSerializer.Deserialize<PaymentFailedEvent>(
                messageJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )
                ?? throw new InvalidOperationException("Failed to deserialize PaymentFailedEvent");

            var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<PaymentFailedEvent>>();

            await ProcessEventAsync(
                dbContext,
                eventId,
                () => handler.HandleAsync(@event, cancellationToken),
                cancellationToken
            );
        }
        else
        {
            var @event = JsonSerializer.Deserialize<PaymentSucceededEvent>(
                messageJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )
                ?? throw new InvalidOperationException("Failed to deserialize PaymentSucceededEvent");

            var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<PaymentSucceededEvent>>();

            await ProcessEventAsync(
                dbContext,
                eventId,
                () => handler.HandleAsync(@event, cancellationToken),
                cancellationToken
            );
        }

        Logger.LogInformation("Inventory consumer processed payment event ");
    }
}
