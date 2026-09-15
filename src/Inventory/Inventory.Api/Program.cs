using Inventory.Application.Abstractions;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DB_CONNECTION_STRING"]
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHostedService<Inventory.Infrastructure.BackgroundServices.OutboxProcessorService>();
builder.Services.AddHostedService<Inventory.Infrastructure.BackgroundServices.OrderEventConsumerService>();
builder.Services.AddHostedService<Inventory.Infrastructure.BackgroundServices.PaymentEventConsumerService>();

builder.Services.AddScoped<IStockService, StockService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await Shared.Infrastructure.Persistence.DatabaseInitializer.EnsureDatabaseCreatedAsync(connectionString);
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
