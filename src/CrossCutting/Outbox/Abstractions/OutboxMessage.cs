namespace CleanArch.CrossCutting.Outbox.Abstractions;

/// <summary>
/// Processing status of an outbox message.
/// </summary>
public enum OutboxMessageStatus
{
    Pending = 0,
    Processed = 1,
    Failed = 2
}

/// <summary>
/// Outbox message entity — stores domain events that must be published reliably.
/// Guarantees at-least-once delivery via the transactional outbox pattern.
///
/// State transitions:
///   Pending → Processed  (happy path)
///   Pending → Pending    (transient failure, retry with exponential backoff)
///   Pending → Failed     (max retries exceeded — dead-lettered)
/// </summary>
public sealed class OutboxMessage
{
    // EF Core requires a parameterless constructor
    private OutboxMessage() { }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? NextRetryOnUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public OutboxMessageStatus Status { get; private set; }

    /// <summary>
    /// Factory method — the only way to create an outbox message.
    /// Called by the <see cref="Interceptors.OutboxInterceptor"/> during pre-save.
    /// </summary>
    public static OutboxMessage Create(string eventType, string payload, DateTime occurredOnUtc)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            OccurredOnUtc = occurredOnUtc,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0
        };
    }

    /// <summary>
    /// Mark as successfully processed.
    /// </summary>
    public void MarkAsProcessed(DateTime utcNow)
    {
        Status = OutboxMessageStatus.Processed;
        ProcessedOnUtc = utcNow;
        Error = null;
    }

    /// <summary>
    /// Record a transient failure and schedule exponential backoff retry.
    /// </summary>
    public void RecordFailure(string error, DateTime utcNow, int maxRetries)
    {
        RetryCount++;
        Error = error;

        if (RetryCount >= maxRetries)
        {
            Status = OutboxMessageStatus.Failed;
            ProcessedOnUtc = utcNow;
        }
        else
        {
            // Exponential backoff: 2^retry * 5 seconds (5s, 10s, 20s, 40s, ...)
            var delaySeconds = Math.Pow(2, RetryCount) * 5;
            NextRetryOnUtc = utcNow.AddSeconds(delaySeconds);
        }
    }
}
