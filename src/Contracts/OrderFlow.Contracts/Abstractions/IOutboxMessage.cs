using System;

namespace OrderFlow.Contracts.Abstractions;

public interface IOutboxMessage
{
    long Id { get; }
    string Topic { get; }
    string Payload { get; }
    DateTime CreatedAt { get; }
    DateTime? PublishedAt { get; set; }
}
