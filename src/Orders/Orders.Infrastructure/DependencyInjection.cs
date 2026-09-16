using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Handlers;
using Orders.Infrastructure.BackgroundServices;
using Orders.Infrastructure.Handlers;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DB_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrderSagaHandler, OrderSagaHandler>();

        services.AddHostedService<OutboxProcessorService>();
        services.AddHostedService<ReservationEventConsumerService>();
        services.AddHostedService<PaymentEventConsumerService>();

        return services;
    }
}
