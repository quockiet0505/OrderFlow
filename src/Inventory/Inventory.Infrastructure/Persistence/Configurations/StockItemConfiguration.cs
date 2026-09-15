using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");
        builder.HasKey(e => e.Sku);
        builder.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(50);
        builder.Property(e => e.QuantityOnHand).HasColumnName("quantity_on_hand").IsRequired().IsConcurrencyToken();
        builder.Property(e => e.QuantityReserved).HasColumnName("quantity_reserved").IsRequired().IsConcurrencyToken();
        builder.Ignore(e => e.Available);

        builder.HasData(
            new StockItem { Sku = "WIDGET-01", QuantityOnHand = 10, QuantityReserved = 0 },
            new StockItem { Sku = "WIDGET-02", QuantityOnHand = 5, QuantityReserved = 0 },
            new StockItem { Sku = "WIDGET-03", QuantityOnHand = 20, QuantityReserved = 0 }
        );
    }
}
