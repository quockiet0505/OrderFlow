using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orders.Domain.Enums;
using Orders.Infrastructure.Inbox;
using Orders.Infrastructure.Persistence;
using OrderFlow.Contracts.Events;
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

        var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        
        if (!root.TryGetProperty("EventId", out var eventIdProp) || !Guid.TryParse(eventIdProp.GetString(), out var eventId)) return;
        if (!root.TryGetProperty("OrderId", out var orderIdProp) || !Guid.TryParse(orderIdProp.GetString(), out var orderId)) return;

        var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == eventId, cancellationToken);
        if (exists) return;

        dbContext.InboxMessages.Add(new InboxMessage { EventId = eventId });

        var order = await dbContext.Orders.Include(x => x.SagaState).FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order != null && order.SagaState != null)
        {
            // // It's ReservationFailedEvent
            if (root.TryGetProperty("Reason", out _)) 
            {
                order.Status = OrderStatus.Cancelled;
            }
            // // It's ReservationSucceededEvent
            else 
            {
                order.SagaState.ReservationCompleted = true;
                order.Status = OrderStatus.Charging; // PDF requirement
                if (order.SagaState.PaymentCompleted)
                {
                    order.Status = OrderStatus.Confirmed;
                }
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        Logger.LogInformation("Orders Saga processed reservation event for OrderId: {OrderId}", orderId);
    }
}
