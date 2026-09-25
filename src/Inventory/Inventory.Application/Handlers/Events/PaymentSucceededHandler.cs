using Inventory.Application.Abstractions;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Events;

namespace Inventory.Application.Handlers;

public class PaymentSucceededHandler : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly IInventoryDbContext _dbContext;
    private readonly ILogger<PaymentSucceededHandler> _logger;

    public PaymentSucceededHandler(IInventoryDbContext dbContext, ILogger<PaymentSucceededHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null || @event.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Received invalid PaymentSucceededEvent payload in Inventory.");
            return;
        }

        var reservations = await _dbContext.Reservations
            .Where(x => x.OrderId == @event.OrderId && x.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        if (reservations.Count == 0)
        {
            _logger.LogInformation("No active reservations found for OrderId ");
            return;
        }

        foreach (var reservation in reservations)
        {
            reservation.Status = ReservationStatus.Consumed;
            var item = await _dbContext.StockItems.FirstOrDefaultAsync(x => x.Sku == reservation.Sku, cancellationToken);
            if (item is not null)
            {
                item.QuantityReserved = Math.Max(0, item.QuantityReserved - reservation.Quantity);
                item.QuantityOnHand = Math.Max(0, item.QuantityOnHand - reservation.Quantity); 
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inventory successfully consumed reserved stock");
    }
}
