using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;
using Payments.Infrastructure.Inbox;
using Payments.Infrastructure.Persistence;
using Shared.Infrastructure.Messaging;
using LocalOutbox = Payments.Infrastructure.Outbox.OutboxMessage;

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

        var reservationEvent = JsonSerializer.Deserialize<ReservationSucceededEvent>(messageJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (reservationEvent == null || reservationEvent.EventId == Guid.Empty) return;
        
        //  check if it has orderId
        if (reservationEvent.OrderId == Guid.Empty) return;

        using var doc = JsonDocument.Parse(messageJson);
        if (doc.RootElement.TryGetProperty("Reason", out _)) 
        {
            //  ReservationFailedEven
            return;
        }

        var exists = await dbContext.InboxMessages.AnyAsync(x => x.EventId == reservationEvent.EventId, cancellationToken);
        if (exists) return;

        dbContext.InboxMessages.Add(new InboxMessage { EventId = reservationEvent.EventId });

        var totalAmount = reservationEvent.Lines.Sum(x => x.Quantity * x.UnitPrice);
        IntegrationEvent outEvent;
        
        var payment = new Payments.Domain.Entities.Payment
        {
            OrderId = reservationEvent.OrderId,
            Amount = totalAmount,
            Status = Payments.Domain.Enums.PaymentStatus.Succeeded
        };

        if (totalAmount % 1 == 0.99m)
        {
            payment.Status = Payments.Domain.Enums.PaymentStatus.Failed;
            outEvent = new PaymentFailedEvent(reservationEvent.OrderId, "Fake gateway rule: amount ends in .99");
        }
        else
        {
            outEvent = new PaymentSucceededEvent(reservationEvent.OrderId, payment.Id, totalAmount);
        }

        dbContext.Payments.Add(payment);

        dbContext.OutboxMessages.Add(new LocalOutbox
        {
            EventId = outEvent.EventId,
            Topic = "persistent://public/default/payment-events",
            Payload = JsonSerializer.Serialize((object)outEvent),
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Payments processed reservation event for OrderId: {OrderId}", reservationEvent.OrderId);
    }
}
