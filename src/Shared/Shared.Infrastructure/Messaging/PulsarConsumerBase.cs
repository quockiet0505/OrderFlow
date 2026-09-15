using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
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
                var data = message.Data.ToArray();
                var json = Encoding.UTF8.GetString(data);
                await ConsumeMessageAsync(_topic, json, stoppingToken);
                await consumer.Acknowledge(message, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing message from topic {Topic}", _topic);
                
                // Fallback DLQ logic
                if (message.RedeliveryCount >= 3)
                {
                    Logger.LogWarning("Message exceeded max redelivery count. Sending to DLQ.");
                    await using var dlqProducer = client.NewProducer().Topic($"{_topic}-dlq").Create();
                    await dlqProducer.Send(message.Data, stoppingToken);
                    await consumer.Acknowledge(message, stoppingToken);
                }
                else
                {
                    // Delay before redelivery in case of transient errors like DB lock
                    await Task.Delay(1000, stoppingToken); 
                }
            }
        }
    }
}
