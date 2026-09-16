using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OrderFlow.Contracts.Events;
using Payments.Application.Handlers;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Infrastructure.Outbox;
using Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Payments.Infrastructure.Handlers;

public class PaymentCommandHandler : IPaymentCommandHandler
{
    private readonly PaymentsDbContext _dbContext;
    private readonly ILogger<PaymentCommandHandler> _logger;

    public PaymentCommandHandler(PaymentsDbContext dbContext, ILogger<PaymentCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(ReservationSucceededEvent @event, CancellationToken cancellationToken = default)
    {
        var existingPayment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.OrderId == @event.OrderId, cancellationToken);

        if (existingPayment != null)
        {
            _logger.LogInformation("Payment for OrderId {OrderId} already processed.", @event.OrderId);
            return;
        }

        decimal totalAmount = @event.Lines.Sum(l => l.Quantity * l.UnitPrice);

        // Fake payment gateway rule: fails if total ends in .99 (e.g., 19.99)
        bool isFailure = Math.Abs((totalAmount * 100) % 100) == 99 || totalAmount.ToString("F2").EndsWith(".99");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = @event.OrderId,
            Amount = totalAmount,
            Status = isFailure ? PaymentStatus.Failed : PaymentStatus.Succeeded,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);

        IntegrationEvent outEvent;
        if (isFailure)
        {
            outEvent = new PaymentFailedEvent(@event.OrderId, "Payment failed by gateway rule (.99 total)");
        }
        else
        {
            outEvent = new PaymentSucceededEvent(@event.OrderId, payment.Id, totalAmount);
        }

        var outboxMessage = new OutboxMessage
        {
            EventId = outEvent.EventId,
            Topic = "persistent://public/default/payment-events",
            Payload = JsonSerializer.Serialize((object)outEvent),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment processed for OrderId: {OrderId}, Status: {Status}", @event.OrderId, payment.Status);
    }
}
