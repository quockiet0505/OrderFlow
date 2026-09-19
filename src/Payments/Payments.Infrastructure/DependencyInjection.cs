using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Contracts.Events;
using Payments.Application.Abstractions;
using Payments.Application.Handlers;
using Payments.Infrastructure.Messaging.Consumers;
using Payments.Infrastructure.Messaging.Publishers;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DB_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPaymentsDbContext>(sp => sp.GetRequiredService<PaymentsDbContext>());

        // Register Integration Event Handler
        services.AddScoped<IIntegrationEventHandler<ReservationSucceededEvent>, ReservationSucceededHandler>();

        // Register API Use Case Handler
        services.AddScoped<IGetPaymentHandler, GetPaymentHandler>();

        services.AddHostedService<OutboxProcessorService>();
        services.AddHostedService<ReservationEventConsumerService>();

        return services;
    }
}
