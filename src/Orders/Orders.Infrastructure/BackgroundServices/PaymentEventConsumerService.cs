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
using Shared.Infrastructure.Messaging;

namespace Orders.Infrastructure.BackgroundServices;

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
            "orders-payment-sub",
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
            if (root.TryGetProperty("Reason", out _)) // It's PaymentFailedEvent
            {
                order.Status = OrderStatus.Cancelled;
            }
            else // It's PaymentSucceededEvent
            {
                order.SagaState.PaymentCompleted = true;
                if (order.SagaState.ReservationCompleted)
                {
                    order.Status = OrderStatus.Completed;
                }
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        Logger.LogInformation("Orders Saga processed payment event for OrderId: {OrderId}", orderId);
    }
}
