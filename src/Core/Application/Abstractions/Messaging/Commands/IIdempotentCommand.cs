using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Application.Abstractions.Messaging.Commands;

/// <summary>
/// Marker interface for commands that require idempotency.
/// Commands implementing this interface are intercepted by the
/// <c>IdempotencyBehavior</c> pipeline, which checks for duplicate
/// submissions using the <see cref="IdempotencyKey"/>.
///
/// Usage:
///   1. Your command record implements <see cref="IIdempotentCommand{TResponse}"/>.
///   2. The controller passes the <c>X-Idempotency-Key</c> header value
///      into the command's <see cref="IdempotencyKey"/> property.
///   3. The pipeline behavior handles the rest.
/// </summary>
public interface IIdempotentCommand : ICommand
{
    Guid IdempotencyKey { get; }
}

/// <summary>
/// Idempotent command returning a typed Result value.
/// </summary>
public interface IIdempotentCommand<TResponse> : ICommand<TResponse>
{
    Guid IdempotencyKey { get; }
}
