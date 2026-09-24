using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;
using Orders.Application.Abstractions;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Handlers;

public class PaymentSucceededHandler : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly IOrdersDbContext _dbContext;
    private readonly ILogger<PaymentSucceededHandler> _logger;

    public PaymentSucceededHandler(IOrdersDbContext dbContext, ILogger<PaymentSucceededHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event is null || @event.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Received invalid PaymentSucceededEvent payload.");
            return;
        }

        var order = await _dbContext.Orders
            .Include(o => o.SagaState)
            .FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Order not found for PaymentSucceededEvent");
            return;
        }

        if (order.Status is not OrderStatus.Confirmed && order.Status is not OrderStatus.Cancelled)
        {
            order.Status = OrderStatus.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;

            order.SagaState ??= new OrderSagaState { OrderId = order.Id };
            order.SagaState.PaymentCompleted = true;
            order.SagaState.LastProcessedEventId = @event.EventId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Order status updated to Confirmed");
        }
    }
}
