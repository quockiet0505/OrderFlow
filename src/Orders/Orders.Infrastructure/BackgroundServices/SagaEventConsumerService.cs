using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Inbox;
using Orders.Infrastructure.Persistence;
using Shared.Infrastructure.Messaging;

namespace Orders.Infrastructure.BackgroundServices;

public class SagaEventConsumerService : PulsarConsumerBase
{
    private readonly IServiceProvider _serviceProvider;

    public SagaEventConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<SagaEventConsumerService> logger)
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

        using var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        
        if (root.TryGetProperty("EventId", out var eventIdProp) && Guid.TryParse(eventIdProp.GetString(), out var eventId))
        {
            var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == eventId, cancellationToken);
            if (exists) return;

            dbContext.InboxMessages.Add(new InboxMessage { EventId = eventId });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        Logger.LogInformation("Orders Saga received reservation event: {Json}", messageJson);
    }
}
