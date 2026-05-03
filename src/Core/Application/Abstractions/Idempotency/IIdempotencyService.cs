namespace CleanArch.Application.Abstractions.Idempotency;

/// <summary>
/// Abstraction for idempotency checking — lives in Application layer
/// so the <c>IdempotencyBehavior</c> does not depend on EF Core or
/// any infrastructure concern.
///
/// Implementation is in the Infrastructure layer and uses EF Core.
/// </summary>
public interface IIdempotencyService
{
    Task<bool> ExistsAsync(Guid idempotencyKey, CancellationToken ct = default);
    Task RecordAsync(Guid idempotencyKey, string commandName, CancellationToken ct = default);
}
