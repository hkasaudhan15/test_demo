namespace CleanArch.CrossCutting.Outbox.Abstractions;

/// <summary>
/// Outbox message entity — stores events that need to be published reliably.
/// Ensures at-least-once delivery of integration events.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }

    public bool IsProcessed => ProcessedOnUtc.HasValue;
}

/// <summary>
/// Outbox message repository interface.
/// </summary>
public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken ct = default);
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken ct = default);
}
