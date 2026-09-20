using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Contracts.Events;
using Orders.Application.Abstractions;
using Orders.Application.Handlers;
using Orders.Infrastructure.Messaging.Consumers;
using Orders.Infrastructure.Messaging.Publishers;
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

        services.AddScoped<IOrdersDbContext>(sp => sp.GetRequiredService<OrdersDbContext>());

        services.AddScoped<IIntegrationEventHandler<ReservationSucceededEvent>, ReservationSucceededHandler>();
        services.AddScoped<IIntegrationEventHandler<ReservationFailedEvent>, ReservationFailedHandler>();
        services.AddScoped<IIntegrationEventHandler<PaymentSucceededEvent>, PaymentSucceededHandler>();
        services.AddScoped<IIntegrationEventHandler<PaymentFailedEvent>, PaymentFailedHandler>();

        services.AddScoped<ICreateOrderHandler, CreateOrderHandler>();
        services.AddScoped<IGetOrderHandler, GetOrderHandler>();
        services.AddScoped<IGetOrdersByCustomerHandler, GetOrdersByCustomerHandler>();

        services.AddHostedService<OutboxProcessorService>();
        services.AddHostedService<ReservationEventConsumerService>();
        services.AddHostedService<PaymentEventConsumerService>();

        return services;
    }
}
