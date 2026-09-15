using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CustomerId).HasColumnName("customer_id").HasMaxLength(100).IsRequired();
        builder.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(10, 2).IsRequired();
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(e => e.Lines)
            .WithOne(l => l.Order)
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SagaState)
            .WithOne(s => s.Order)
            .HasForeignKey<OrderSagaState>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
