using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payments.Infrastructure.Persistence;
using Shared.Infrastructure.Outbox;
using LocalOutbox = Payments.Infrastructure.Outbox.OutboxMessage;

namespace Payments.Infrastructure.BackgroundServices;

public class OutboxProcessorService : OutboxProcessorBase<PaymentsDbContext, LocalOutbox>
{
    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OutboxProcessorService> logger)
        : base(
            serviceProvider,
            configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650",
            "persistent://public/default/payment-events",
            logger)
    {
    }

    protected override DbSet<LocalOutbox> GetOutboxDbSet(PaymentsDbContext dbContext)
    {
        return dbContext.OutboxMessages;
    }

    protected override bool IsProcessed(LocalOutbox message) => message.PublishedAt != null;
    protected override string GetPayload(LocalOutbox message) => message.Payload;
    protected override void MarkAsProcessed(LocalOutbox message) => message.PublishedAt = DateTime.UtcNow;
}
