using System;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Succeeded;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
