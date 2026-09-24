using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Abstractions;

namespace Shared.Infrastructure.Messaging;

public abstract class OutboxProcessorBase<TDbContext, TOutboxMessage> : BackgroundService
    where TDbContext : DbContext
    where TOutboxMessage : class, IOutboxMessage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPulsarClient _pulsarClient;
    private readonly string _topic;
    protected readonly ILogger Logger;

    protected OutboxProcessorBase(
        IServiceProvider serviceProvider,
        IPulsarClient pulsarClient,
        string topic,
        ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _pulsarClient = pulsarClient;
        _topic = topic;
        Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // await Task.Delay(3000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var producer = _pulsarClient.NewProducer()
                    .Topic(_topic)
                    .Create();

                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

                    var messages = await dbContext.Set<TOutboxMessage>()
                        .Where(m => m.PublishedAt == null)
                        .OrderBy(m => m.CreatedAt)
                        .ToListAsync(stoppingToken);

                    if (messages.Count > 0)
                    {
                        foreach (var msg in messages)
                        {
                            var data = Encoding.UTF8.GetBytes(msg.Payload);
                            await producer.Send(data, stoppingToken);

                            msg.PublishedAt = DateTime.UtcNow;
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }

                    await Task.Delay(2000, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Outbox processor warning on '{Topic}': {Message}", _topic, ex.Message);
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}
