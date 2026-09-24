using Orders.Api;
using Orders.Infrastructure;
using Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DB_CONNECTION_STRING"]
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

using (var scope = app.Services.CreateScope())
{
    await Shared.Infrastructure.Persistence.DatabaseInitializer.EnsureDatabaseCreatedAsync(connectionString);
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
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
