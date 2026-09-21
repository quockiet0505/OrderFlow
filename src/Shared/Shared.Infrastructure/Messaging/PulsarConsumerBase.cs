using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Messaging;

public abstract class PulsarConsumerBase : BackgroundService
{
    private readonly IPulsarClient _pulsarClient;
    private readonly string _topic;
    private readonly string _subscription;
    protected readonly ILogger Logger;

    protected PulsarConsumerBase(
        IPulsarClient pulsarClient, 
        string topic, 
        string subscription, 
        ILogger logger)
    {
        _pulsarClient = pulsarClient;
        _topic = topic;
        _subscription = subscription;
        Logger = logger;
    }

    protected abstract Task ConsumeMessageAsync(
        string topic, 
        string messageJson, 
        CancellationToken cancellationToken
    );

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var consumer = _pulsarClient.NewConsumer()
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
                        
                        if (message.RedeliveryCount >= 3)
                        {
                            Logger.LogWarning("Message exceeded max redelivery count. Sending to DLQ.");
                            await using var dlqProducer = _pulsarClient.NewProducer().Topic($"{_topic}-dlq").Create();
                            await dlqProducer.Send(message.Data, stoppingToken);
                            await consumer.Acknowledge(message, stoppingToken);
                        }
                        else
                        {
                            await Task.Delay(1000, stoppingToken); 
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Pulsar consumer encountered an exception on topic '{Topic}'. Retrying connection in 3s...", _topic);
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}

