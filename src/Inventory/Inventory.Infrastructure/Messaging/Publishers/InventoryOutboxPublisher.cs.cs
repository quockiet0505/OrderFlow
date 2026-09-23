using System;
using DotPulsar.Abstractions;
using Inventory.Infrastructure.Persistence;
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
}
