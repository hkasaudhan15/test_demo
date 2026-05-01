namespace CleanArch.CrossCutting.Idempotency;

/// <summary>
/// Stores processed idempotent requests to prevent duplicate processing.
/// </summary>
public sealed class IdempotentRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
}

/// <summary>
/// Marker interface for commands that require idempotency.
/// </summary>
public interface IIdempotentCommand
{
    Guid IdempotencyKey { get; }
}
