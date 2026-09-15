using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(e => e.UnitPrice).HasColumnName("unit_price").HasPrecision(10, 2).IsRequired();
    }
}
