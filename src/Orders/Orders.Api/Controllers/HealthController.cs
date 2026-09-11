using System.Threading.Tasks;
using Orders.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Orders.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly OrdersDbContext _dbContext;

    public HealthController(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Health check
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        bool canConnect = await _dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            return Ok(new { status = "Healthy", database = "Connected" });
        }
        return StatusCode(503, new { status = "Unhealthy", database = "Disconnected" });
    }
}
