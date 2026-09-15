using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Persistence;
using Shared.Infrastructure.Outbox;
using LocalOutbox = Orders.Infrastructure.Outbox.OutboxMessage;

namespace Orders.Infrastructure.BackgroundServices;

public class OutboxProcessorService : OutboxProcessorBase<OrdersDbContext, LocalOutbox>
{
    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OutboxProcessorService> logger)
        : base(
            serviceProvider,
            configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650",
            "persistent://public/default/order-events",
            logger)
    {
    }

    protected override DbSet<LocalOutbox> GetOutboxDbSet(OrdersDbContext dbContext)
    {
        return dbContext.OutboxMessages;
    }

    protected override bool IsProcessed(LocalOutbox message) => message.PublishedAt != null;
    protected override string GetPayload(LocalOutbox message) => message.Payload;
    protected override void MarkAsProcessed(LocalOutbox message) => message.PublishedAt = DateTime.UtcNow;
}
