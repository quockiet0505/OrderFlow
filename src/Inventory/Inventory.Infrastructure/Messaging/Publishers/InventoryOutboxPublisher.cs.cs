using System;
using DotPulsar.Abstractions;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Constants;
using Shared.Infrastructure.Messaging;
using LocalOutbox = Inventory.Domain.Entities.OutboxMessage;

namespace Inventory.Infrastructure.Messaging.Publishers;

public class OutboxProcessorService : OutboxProcessorBase<InventoryDbContext, LocalOutbox>
{
    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        IPulsarClient pulsarClient,
        ILogger<OutboxProcessorService> logger)
        : base(
            serviceProvider,
            pulsarClient,
            PulsarTopics.ReservationEvents,
            logger)
    {
    }

    protected override DbSet<LocalOutbox> GetOutboxDbSet(InventoryDbContext dbContext)
    {
        return dbContext.OutboxMessages;
    }

    protected override bool IsProcessed(LocalOutbox message) => message.PublishedAt != null;
    protected override string GetPayload(LocalOutbox message) => message.Payload;
    protected override void MarkAsProcessed(LocalOutbox message) => message.PublishedAt = DateTime.UtcNow;
}
