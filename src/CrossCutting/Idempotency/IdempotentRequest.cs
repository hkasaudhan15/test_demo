namespace CleanArch.CrossCutting.Idempotency;

/// <summary>
/// Records a processed idempotent request so duplicate submissions
/// (same <see cref="IdempotencyKey"/>) are rejected or return
/// the previously stored response.
///
/// Lifecycle:
///   1. Client sends a command with <c>X-Idempotency-Key</c> header.
///   2. <see cref="IdempotencyBehavior{TRequest,TResponse}"/> checks this table.
///   3. If the key exists → return <see cref="ErrorCodes.Idempotency.AlreadyProcessed"/>.
///   4. If absent → execute the command, then insert a row here.
/// </summary>
public sealed class IdempotentRequest
{
    private IdempotentRequest() { } // EF Core

    public Guid Id { get; private set; }
    public string CommandName { get; private set; } = string.Empty;
    public DateTime CreatedOnUtc { get; private set; }

    public static IdempotentRequest Create(Guid idempotencyKey, string commandName, DateTime utcNow)
    {
        return new IdempotentRequest
        {
            Id = idempotencyKey,
            CommandName = commandName,
            CreatedOnUtc = utcNow
        };
    }
}
