using OrderFlow.Contracts.Abstractions;

namespace Orders.Domain.Entities;

public class OutboxMessage : IOutboxMessage
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}
