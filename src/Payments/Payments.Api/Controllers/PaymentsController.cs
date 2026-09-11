using System;
using System.Threading.Tasks;
using Payments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Payments.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentsDbContext _dbContext;

    public PaymentsController(PaymentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Get payment by order ID
    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetPaymentByOrderId(Guid orderId)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (payment == null)
        {
            return NotFound(new { message = $"No payment record found for orderId: {orderId}" });
        }

        return Ok(new
        {
            paymentId = payment.Id,
            orderId = payment.OrderId,
            amount = payment.Amount,
            status = payment.Status.ToString(),
            createdAt = payment.CreatedAt
        });
    }
}
