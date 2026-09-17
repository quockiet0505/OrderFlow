using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext, IPaymentsDbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
    }
}
