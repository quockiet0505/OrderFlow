using Inventory.Application.Abstractions;
using Inventory.Application.Handlers;
using Inventory.Infrastructure.Messaging.Consumers;
using Inventory.Infrastructure.Messaging.Publishers;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Contracts.Events;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DB_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IInventoryDbContext>(sp => sp.GetRequiredService<InventoryDbContext>());

        services.AddSingleton<DotPulsar.Abstractions.IPulsarClient>(sp =>
        {
            var pulsarUrl = configuration["Pulsar:ServiceUrl"] ?? OrderFlow.Contracts.Constants.PulsarDefaults.DefaultServiceUrl;
            return DotPulsar.PulsarClient.Builder().ServiceUrl(new Uri(pulsarUrl)).Build();
        });

        services.AddScoped<
            IIntegrationEventHandler<OrderPlacedEvent>, 
            OrderPlacedHandler>();
        services.AddScoped<
            IIntegrationEventHandler<PaymentSucceededEvent>, 
            PaymentSucceededHandler>();
        services.AddScoped<
            IIntegrationEventHandler<PaymentFailedEvent>, 
            PaymentFailedHandler>();

        services.AddHostedService<OutboxProcessorService>();
        services.AddHostedService<OrderEventConsumerService>();
        services.AddHostedService<PaymentEventConsumerService>();

        return services;
    }
}
