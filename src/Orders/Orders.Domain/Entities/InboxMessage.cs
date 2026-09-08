using System;

namespace Orders.Domain.Entities;

public class InboxMessage
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
