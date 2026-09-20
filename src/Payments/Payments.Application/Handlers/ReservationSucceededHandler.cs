using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;
using Payments.Application.Abstractions;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Application.Handlers;

public class ReservationSucceededHandler : IIntegrationEventHandler<ReservationSucceededEvent>
{
    private readonly IPaymentsDbContext _dbContext;
    private readonly ILogger<ReservationSucceededHandler> _logger;

    public ReservationSucceededHandler(IPaymentsDbContext dbContext, ILogger<ReservationSucceededHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(ReservationSucceededEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null || @event.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Received invalid ReservationSucceededEvent payload in Payments.");
            return;
        }

        if (@event.Lines == null || !@event.Lines.Any())
        {
            _logger.LogWarning("ReservationSucceededEvent for OrderId {OrderId} contains no reserved lines.", @event.OrderId);
            return;
        }

        foreach (var line in @event.Lines)
        {
            if (line.Quantity <= 0 || line.UnitPrice < 0)
            {
                _logger.LogWarning("Invalid line values in ReservationSucceededEvent for OrderId {OrderId}: SKU={Sku}, Qty={Qty}, Price={Price}",
                    @event.OrderId, line.Sku, line.Quantity, line.UnitPrice);
                return;
            }
        }

        var existingPayment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.OrderId == @event.OrderId, cancellationToken);

        if (existingPayment != null)
        {
            _logger.LogInformation("Payment for OrderId {OrderId} already processed. Skipping duplicated processing.", @event.OrderId);
            return;
        }

        decimal totalAmount = @event.Lines.Sum(l => l.Quantity * l.UnitPrice);

        bool isFailure = Math.Abs((totalAmount * 100) % 100) == 99;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = @event.OrderId,
            Amount = totalAmount,
            Status = isFailure ? PaymentStatus.Failed : PaymentStatus.Succeeded,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);

        IntegrationEvent outEvent = isFailure
            ? new PaymentFailedEvent(@event.OrderId, "Payment failed by gateway rule (.99 total)")
            : new PaymentSucceededEvent(@event.OrderId, payment.Id, totalAmount);

        var outboxMessage = new OutboxMessage
        {
            EventId = outEvent.EventId,
            Topic = "persistent://public/default/payment-events",
            Payload = JsonSerializer.Serialize((object)outEvent),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment processed for OrderId: {OrderId}, Total: {Amount}, Status: {Status}",
            @event.OrderId, totalAmount, payment.Status);
    }
}
