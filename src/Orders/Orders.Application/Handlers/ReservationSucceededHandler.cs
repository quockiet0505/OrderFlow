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

public class ReservationSucceededHandler : IIntegrationEventHandler<ReservationSucceededEvent>
{
    private readonly IOrdersDbContext _dbContext;
    private readonly ILogger<ReservationSucceededHandler> _logger;

    public ReservationSucceededHandler(IOrdersDbContext dbContext, ILogger<ReservationSucceededHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(ReservationSucceededEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null || @event.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Received invalid ReservationSucceededEvent payload.");
            return;
        }

        var order = await _dbContext.Orders
            .Include(o => o.SagaState)
            .FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for ReservationSucceededEvent", @event.OrderId);
            return;
        }

        if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Reserving)
        {
            order.Status = OrderStatus.Charging;
            order.UpdatedAt = DateTime.UtcNow;

            order.SagaState ??= new OrderSagaState { OrderId = order.Id };
            order.SagaState.ReservationCompleted = true;
            order.SagaState.LastProcessedEventId = @event.EventId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Order {OrderId} status updated to Charging", order.Id);
        }
        else
        {
            _logger.LogInformation("Order {OrderId} already in status {Status}, ignoring late ReservationSucceededEvent", order.Id, order.Status);
        }
    }
}
