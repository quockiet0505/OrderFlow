using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // OutboxMessage Configuration
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.EventId).HasColumnName("event_id").IsRequired();
            entity.HasIndex(e => e.EventId).IsUnique();
            entity.Property(e => e.Topic).HasColumnName("topic").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
        });

        //InboxMessage Configuration
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages");
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        });
    }
}
