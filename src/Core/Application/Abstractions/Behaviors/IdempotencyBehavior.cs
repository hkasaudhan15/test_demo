using CleanArch.Application.Abstractions.Idempotency;
using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Abstractions.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces idempotency for commands
/// implementing <see cref="IIdempotentCommand"/> or <see cref="IIdempotentCommand{TResponse}"/>.
///
/// Flow:
///   1. Extract <c>IdempotencyKey</c> from the command.
///   2. Query the idempotency store for a matching key.
///   3. If found → short-circuit with a <c>Conflict</c> error (duplicate).
///   4. If absent → execute the command, then record the key.
///
/// The check-then-insert is not fully atomic (a race condition exists between
/// step 2 and 4), but the unique PK constraint on the underlying table
/// guarantees that at most one request succeeds — duplicates fail silently.
/// </summary>
public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

    public IdempotencyBehavior(
        IIdempotencyService idempotencyService,
        ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    {
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var idempotencyKey = GetIdempotencyKey(request);
        if (idempotencyKey is null)
            return await next();

        return await ProcessIdempotentRequest(idempotencyKey.Value, next, ct);
    }

    private async Task<TResponse> ProcessIdempotentRequest(
        Guid idempotencyKey,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var commandName = typeof(TRequest).Name;

        if (await _idempotencyService.ExistsAsync(idempotencyKey, ct))
        {
            _logger.LogInformation(
                "Duplicate idempotent request: {CommandName} key={IdempotencyKey}",
                commandName, idempotencyKey);

            return CreateConflictResult(commandName, idempotencyKey);
        }

        var response = await next();

        if (response.IsSuccess)
        {
            await _idempotencyService.RecordAsync(idempotencyKey, commandName, ct);
        }

        return response;
    }

    private static Guid? GetIdempotencyKey(TRequest request)
    {
        return request switch
        {
            IIdempotentCommand cmd => cmd.IdempotencyKey,
            _ => GetIdempotencyKeyFromGenericInterface(request)
        };
    }

    private static Guid? GetIdempotencyKeyFromGenericInterface(TRequest request)
    {
        foreach (var iface in request.GetType().GetInterfaces())
        {
            if (iface.IsGenericType &&
                iface.GetGenericTypeDefinition() == typeof(IIdempotentCommand<>))
            {
                var property = iface.GetProperty(nameof(IIdempotentCommand.IdempotencyKey));
                if (property?.GetValue(request) is Guid key)
                    return key;
            }
        }

        return null;
    }

    private static TResponse CreateConflictResult(string commandName, Guid idempotencyKey)
    {
        var error = new Error(
            "Idempotency.AlreadyProcessed",
            $"Request '{commandName}' with idempotency key '{idempotencyKey}' has already been processed.",
            ErrorType.Conflict);

        if (typeof(TResponse) == typeof(Result))
            return (TResponse)Result.Failure(error);

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}
