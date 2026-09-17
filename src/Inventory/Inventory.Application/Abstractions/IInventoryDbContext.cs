using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities;

namespace Inventory.Application.Abstractions;

public interface IInventoryDbContext
{
    DbSet<StockItem> StockItems { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<InboxMessage> InboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
