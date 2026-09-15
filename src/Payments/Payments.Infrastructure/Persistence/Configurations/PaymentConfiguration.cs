using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
        builder.HasIndex(e => e.OrderId).IsUnique();
        builder.Property(e => e.Amount).HasColumnName("amount").HasPrecision(10, 2).IsRequired();
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
