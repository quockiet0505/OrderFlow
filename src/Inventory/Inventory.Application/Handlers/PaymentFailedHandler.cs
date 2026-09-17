using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Application.Abstractions;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;

namespace Inventory.Application.Handlers;

public class PaymentFailedHandler : IIntegrationEventHandler<PaymentFailedEvent>
{
    private readonly IInventoryDbContext _dbContext;
    private readonly ILogger<PaymentFailedHandler> _logger;

    public PaymentFailedHandler(IInventoryDbContext dbContext, ILogger<PaymentFailedHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentFailedEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null || @event.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Received invalid PaymentFailedEvent payload in Inventory.");
            return;
        }

        var reservations = await _dbContext.Reservations
            .Where(x => x.OrderId == @event.OrderId && x.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        if (!reservations.Any())
        {
            _logger.LogInformation("No active reservations found for OrderId {OrderId} on PaymentFailedEvent (may already be released).", @event.OrderId);
            return;
        }

        foreach (var reservation in reservations)
        {
            reservation.Status = ReservationStatus.Released;
            var item = await _dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == reservation.Sku, cancellationToken);
            if (item != null)
            {
                item.QuantityReserved = Math.Max(0, item.QuantityReserved - reservation.Quantity); // Restore available stock
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inventory successfully compensated (released) reserved stock for OrderId: {OrderId}", @event.OrderId);
    }
}
