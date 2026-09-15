using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence.Configurations;

public class OrderSagaStateConfiguration : IEntityTypeConfiguration<OrderSagaState>
{
    public void Configure(EntityTypeBuilder<OrderSagaState> builder)
    {
        builder.ToTable("order_saga_state");
        builder.HasKey(e => e.OrderId);
        builder.Property(e => e.OrderId).HasColumnName("order_id");
        builder.Property(e => e.ReservationCompleted).HasColumnName("reservation_completed");
        builder.Property(e => e.PaymentCompleted).HasColumnName("payment_completed");
        builder.Property(e => e.LastProcessedEventId).HasColumnName("last_processed_event_id");
    }
}
