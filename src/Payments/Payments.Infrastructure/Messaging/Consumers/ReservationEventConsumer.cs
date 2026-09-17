using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;
using Payments.Domain.Entities;
using Payments.Infrastructure.Persistence;
using Shared.Infrastructure.Messaging;

namespace Payments.Infrastructure.Messaging.Consumers;

public class ReservationEventConsumerService : PulsarConsumerBase
{
    private readonly IServiceProvider _serviceProvider;

    public ReservationEventConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ReservationEventConsumerService> logger)
        : base(
            configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650",
            "persistent://public/default/reservation-events",
            "payments-reservation-sub",
            logger)
    {
        _serviceProvider = serviceProvider;
    }

    private static async Task ProcessEventAsync(
        PaymentsDbContext dbContext,
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
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<ReservationSucceededEvent>>();

        using var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        
        // Ignore ReservationFailedEvent
        if (root.TryGetProperty("Reason", out _)) return;

        if (!root.TryGetProperty("EventId", out var eventIdProp) || !Guid.TryParse(eventIdProp.GetString(), out var eventId)) return;

        var reservationEvent = JsonSerializer.Deserialize<ReservationSucceededEvent>(
            messageJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (reservationEvent is null)
        {
            Logger.LogWarning("Failed to deserialize ReservationSucceededEvent from message: {MessageJson}", messageJson);
            return;
        }
        
        await ProcessEventAsync(
            dbContext,
            eventId,
            () => handler.HandleAsync(reservationEvent, cancellationToken),
            cancellationToken
        );

        Logger.LogInformation("Payments consumer processed reservation event EventId: {EventId}", eventId);
    }
}
