using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Messaging;

public abstract class PulsarConsumerBase : BackgroundService
{
    private readonly string _pulsarUrl;
    private readonly string _topic;
    private readonly string _subscription;
    protected readonly ILogger Logger;

    protected PulsarConsumerBase(string pulsarUrl, string topic, string subscription, ILogger logger)
    {
        _pulsarUrl = pulsarUrl;
        _topic = topic;
        _subscription = subscription;
        Logger = logger;
    }

    protected abstract Task ConsumeMessageAsync(string topic, string messageJson, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);

        await using var client = PulsarClient.Builder()
            .ServiceUrl(new Uri(_pulsarUrl))
            .Build();

        await using var consumer = client.NewConsumer()
            .Topic(_topic)
            .SubscriptionName(_subscription)
            .SubscriptionType(SubscriptionType.Shared)
            .Create();

        Logger.LogInformation("Pulsar consumer started on topic '{Topic}', subscription '{Subscription}'", _topic, _subscription);

        await foreach (var message in consumer.Messages(stoppingToken))
        {
            try
            {
                var json = Encoding.UTF8.GetString(message.Data.FirstSpan);
                await ConsumeMessageAsync(_topic, json, stoppingToken);
                await consumer.Acknowledge(message, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing message from topic {Topic}", _topic);
            }
        }
    }
}
