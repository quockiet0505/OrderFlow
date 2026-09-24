using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Abstractions;

namespace Payments.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IGetPaymentHandler _getPaymentHandler;

    public PaymentsController(IGetPaymentHandler getPaymentHandler)
    {
        _getPaymentHandler = getPaymentHandler;
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid Order ID." });
        }

        var payment = await _getPaymentHandler.HandleAsync(orderId, cancellationToken);
        if (payment == null)
        {
            return NotFound(new { message = $"No payment record found for orderId: {orderId}" });
        }

        return Ok(payment);
    }
}
