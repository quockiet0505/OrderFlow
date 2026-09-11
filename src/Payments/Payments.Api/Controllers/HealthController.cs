using System.Threading.Tasks;
using Payments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Payments.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly PaymentsDbContext _dbContext;

    public HealthController(PaymentsDbContext dbContext)
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
