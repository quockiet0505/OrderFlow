using DotPulsar.Abstractions;
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
}
