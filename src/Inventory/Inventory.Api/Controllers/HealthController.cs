using System.Threading.Tasks;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly InventoryDbContext _dbContext;

    public HealthController(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET /health
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
