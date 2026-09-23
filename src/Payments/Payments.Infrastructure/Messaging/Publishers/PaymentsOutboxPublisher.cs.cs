using System;
using DotPulsar.Abstractions;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Constants;
using Payments.Infrastructure.Persistence;
using Shared.Infrastructure.Messaging;
using LocalOutbox = Payments.Domain.Entities.OutboxMessage;

namespace Payments.Infrastructure.Messaging.Publishers;

public class OutboxProcessorService : OutboxProcessorBase<PaymentsDbContext, LocalOutbox>
{
    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        IPulsarClient pulsarClient,
        ILogger<OutboxProcessorService> logger)
        : base(
            serviceProvider,
            pulsarClient,
            PulsarTopics.PaymentEvents,
            logger)
    {
    }
}
