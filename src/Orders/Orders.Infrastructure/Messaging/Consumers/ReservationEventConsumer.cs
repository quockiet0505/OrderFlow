using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;
using Orders.Application.Handlers;
using Orders.Infrastructure.Inbox;
using Orders.Infrastructure.Persistence;
using Shared.Infrastructure.Messaging;

namespace Orders.Infrastructure.BackgroundServices;

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
            "orders-saga-sub",
            logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ConsumeMessageAsync(string topic, string messageJson, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        var sagaHandler = scope.ServiceProvider.GetRequiredService<IOrderSagaHandler>();

        using var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        
        if (!root.TryGetProperty("EventId", out var eventIdProp) || !Guid.TryParse(eventIdProp.GetString(), out var eventId)) return;

        var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == eventId, cancellationToken);
        if (exists) return;

        dbContext.InboxMessages.Add(new InboxMessage { EventId = eventId, ProcessedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);

        if (root.TryGetProperty("Reason", out _)) 
        {
            var @event = JsonSerializer.Deserialize<ReservationFailedEvent>(messageJson);
            if (@event != null)
            {
                await sagaHandler.HandleAsync(@event, cancellationToken);
            }
        }
        else 
        {
            var @event = JsonSerializer.Deserialize<ReservationSucceededEvent>(messageJson);
            if (@event != null)
            {
                await sagaHandler.HandleAsync(@event, cancellationToken);
            }
        }

        Logger.LogInformation("Orders Saga processed reservation event EventId: {EventId}", eventId);
    }
}
