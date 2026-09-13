using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Payments.Infrastructure.Inbox;
using Payments.Infrastructure.Persistence;
using Shared.Infrastructure.Messaging;

namespace Payments.Infrastructure.BackgroundServices;

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

    protected override async Task ConsumeMessageAsync(string topic, string messageJson, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        using var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        
        if (root.TryGetProperty("EventId", out var eventIdProp) && Guid.TryParse(eventIdProp.GetString(), out var eventId))
        {
            var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == eventId, cancellationToken);
            if (exists) return;

            dbContext.InboxMessages.Add(new InboxMessage { EventId = eventId });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        Logger.LogInformation("Payments processed reservation event: {Json}", messageJson);
    }
}
