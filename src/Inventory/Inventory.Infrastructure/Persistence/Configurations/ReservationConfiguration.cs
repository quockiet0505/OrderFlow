using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(e => new { e.OrderId, e.Sku }).IsUnique();
    }
}
