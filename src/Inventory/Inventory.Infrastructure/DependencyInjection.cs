using System;
using Inventory.Application.Abstractions;
using Inventory.Application.Handlers;
using Inventory.Infrastructure.BackgroundServices;
using Inventory.Infrastructure.Handlers;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IInventoryCommandHandler, InventoryCommandHandler>();

        services.AddHostedService<OutboxProcessorService>();
        services.AddHostedService<OrderEventConsumerService>();
        services.AddHostedService<PaymentEventConsumerService>();

        return services;
    }
}
