using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;

namespace Payments.Application.Abstractions;

public interface IPaymentsDbContext
{
    DbSet<Payment> Payments { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<InboxMessage> InboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
