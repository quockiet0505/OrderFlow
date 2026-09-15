using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Outbox;

public abstract class OutboxProcessorBase<TDbContext, TOutboxMessage> : BackgroundService 
    where TDbContext : DbContext 
    where TOutboxMessage : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _pulsarUrl;
    private readonly string _topic;
    protected readonly ILogger Logger;

    protected OutboxProcessorBase(IServiceProvider serviceProvider, string pulsarUrl, string topic, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _pulsarUrl = pulsarUrl;
        _topic = topic;
        Logger = logger;
    }

    protected abstract DbSet<TOutboxMessage> GetOutboxDbSet(TDbContext dbContext);
    protected abstract bool IsProcessed(TOutboxMessage message);
    protected abstract string GetPayload(TOutboxMessage message);
    protected abstract void MarkAsProcessed(TOutboxMessage message);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);

        await using var client = PulsarClient.Builder()
            .ServiceUrl(new Uri(_pulsarUrl))
            .Build();

        await using var producer = client.NewProducer()
            .Topic(_topic)
            .Create();

        Logger.LogInformation("Outbox Processor started publishing to Pulsar topic '{Topic}'", _topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

                var messages = await GetOutboxDbSet(dbContext)
                    .ToListAsync(stoppingToken);

                int processedCount = 0;
                foreach (var msg in messages)
                {
                    if (IsProcessed(msg)) continue;

                    var payload = GetPayload(msg);
                    var data = Encoding.UTF8.GetBytes(payload);
                    await producer.Send(data, stoppingToken);

                    MarkAsProcessed(msg);
                    processedCount++;

                    if (processedCount >= 20) break;
                }

                if (processedCount > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing outbox messages for topic {Topic}", _topic);
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}
