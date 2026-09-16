using System;
using System.Threading;
using System.Threading.Tasks;
using OrderFlow.Contracts.Events;
using Orders.Application.Handlers;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Orders.Infrastructure.Handlers;

public class OrderSagaHandler : IOrderSagaHandler
{
    private readonly OrdersDbContext _dbContext;
    private readonly ILogger<OrderSagaHandler> _logger;

    public OrderSagaHandler(OrdersDbContext dbContext, ILogger<OrderSagaHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(ReservationSucceededEvent @event, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.SagaState)
            .FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for ReservationSucceededEvent", @event.OrderId);
            return;
        }

        order.Status = OrderStatus.Charging;
        order.UpdatedAt = DateTime.UtcNow;
        if (order.SagaState != null)
        {
            order.SagaState.ReservationCompleted = true;
            order.SagaState.LastProcessedEventId = @event.EventId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderId} status updated to Charging", order.Id);
    }

    public async Task HandleAsync(ReservationFailedEvent @event, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.SagaState)
            .FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for ReservationFailedEvent", @event.OrderId);
            return;
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        if (order.SagaState != null)
        {
            order.SagaState.LastProcessedEventId = @event.EventId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderId} status updated to Cancelled due to reservation failure", order.Id);
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.SagaState)
            .FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for PaymentSucceededEvent", @event.OrderId);
            return;
        }

        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;
        if (order.SagaState != null)
        {
            order.SagaState.PaymentCompleted = true;
            order.SagaState.LastProcessedEventId = @event.EventId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderId} status updated to Confirmed", order.Id);
    }

    public async Task HandleAsync(PaymentFailedEvent @event, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.SagaState)
            .FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for PaymentFailedEvent", @event.OrderId);
            return;
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        if (order.SagaState != null)
        {
            order.SagaState.LastProcessedEventId = @event.EventId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderId} status updated to Cancelled due to payment failure", order.Id);
    }
}
