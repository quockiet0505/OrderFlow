using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Handlers;
using Payments.Infrastructure.BackgroundServices;
using Payments.Infrastructure.Handlers;
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

        services.AddScoped<IPaymentCommandHandler, PaymentCommandHandler>();

        services.AddHostedService<OutboxProcessorService>();
        services.AddHostedService<ReservationEventConsumerService>();

        return services;
    }
}
