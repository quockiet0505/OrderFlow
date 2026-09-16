using System;
using System.Threading.Tasks;
using DotPulsar;
using Orders.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Orders.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly OrdersDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public HealthController(OrdersDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        bool dbHealthy = await _dbContext.Database.CanConnectAsync();
        bool pulsarHealthy = false;

        try
        {
            var pulsarUrl = _configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650";
            await using var client = PulsarClient.Builder().ServiceUrl(new Uri(pulsarUrl)).Build();
            pulsarHealthy = client != null;
        }
        catch
        {
            pulsarHealthy = false;
        }

        if (dbHealthy && pulsarHealthy)
        {
            return Ok(new { status = "Healthy", database = "Connected", pulsar = "Connected" });
        }

        return StatusCode(503, new
        {
            status = "Unhealthy",
            database = dbHealthy ? "Connected" : "Disconnected",
            pulsar = pulsarHealthy ? "Connected" : "Disconnected"
        });
    }
}
