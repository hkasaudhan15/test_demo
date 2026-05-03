using CleanArch.Domain.Primitives.Results;
using MediatR;

namespace CleanArch.Application.Abstractions.Messaging.Commands;

/// <summary>
/// Marker interface for CQRS Commands — mutate state, return Result.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Command returning a typed Result value.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Command handler — processes a command and returns Result.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Command handler returning a typed Result.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
