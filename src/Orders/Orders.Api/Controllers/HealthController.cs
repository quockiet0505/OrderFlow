using DotPulsar.Abstractions;
using Orders.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Orders.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly OrdersDbContext _dbContext;
    private readonly IPulsarClient _pulsarClient;

    public HealthController(OrdersDbContext dbContext, IPulsarClient pulsarClient)
    {
        _dbContext = dbContext;
        _pulsarClient = pulsarClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealthAsync()
    {
        bool dbHealthy = await _dbContext.Database.CanConnectAsync();
        bool pulsarHealthy = _pulsarClient is not null;

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

