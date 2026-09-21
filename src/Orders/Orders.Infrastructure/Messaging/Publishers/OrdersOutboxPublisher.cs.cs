using System;
using DotPulsar.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Constants;
using Orders.Infrastructure.Persistence;
using Shared.Infrastructure.Messaging;
using LocalOutbox = Orders.Domain.Entities.OutboxMessage;

namespace Orders.Infrastructure.Messaging.Publishers;

public class OutboxProcessorService : OutboxProcessorBase<OrdersDbContext, LocalOutbox>
{
    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        IPulsarClient pulsarClient,
        ILogger<OutboxProcessorService> logger)
        : base(
            serviceProvider,
            pulsarClient,
            PulsarTopics.OrderPlaced,
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
