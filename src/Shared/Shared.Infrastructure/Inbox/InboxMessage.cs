using System;
using System.Text.Json;

namespace Shared.Infrastructure.Inbox;

public class InboxMessage
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
