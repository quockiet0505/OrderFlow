using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;
using Orders.Application.Abstractions;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Handlers;

public class ReservationFailedHandler : IIntegrationEventHandler<ReservationFailedEvent>
{
    private readonly IOrdersDbContext _dbContext;
    private readonly ILogger<ReservationFailedHandler> _logger;

    public ReservationFailedHandler(IOrdersDbContext dbContext, ILogger<ReservationFailedHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(ReservationFailedEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null || @event.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Received invalid ReservationFailedEvent payload.");
            return;
        }

        var order = await _dbContext.Orders
            .Include(o => o.SagaState)
            .FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for ReservationFailedEvent", @event.OrderId);
            return;
        }

        if (order.Status != OrderStatus.Cancelled && order.Status != OrderStatus.Confirmed)
        {
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            order.SagaState ??= new OrderSagaState { OrderId = order.Id };
            order.SagaState.LastProcessedEventId = @event.EventId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Order {OrderId} status updated to Cancelled due to reservation failure. Reason: {Reason}", order.Id, @event.Reason);
        }
    }
}
