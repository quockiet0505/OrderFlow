using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //  StockItem Configuration
        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.ToTable("stock_items");
            entity.HasKey(e => e.Sku);
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(50);
            entity.Property(e => e.QuantityOnHand).HasColumnName("quantity_on_hand").IsRequired();
            entity.Property(e => e.QuantityReserved).HasColumnName("quantity_reserved").IsRequired();
            entity.Ignore(e => e.Available);
        });

        //  Reservation Configuration
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("reservations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => new { e.OrderId, e.Sku }).IsUnique();
        });

        //  OutboxMessage Configuration
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

        //  InboxMessage Configuration
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages");
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        });
    }
}
