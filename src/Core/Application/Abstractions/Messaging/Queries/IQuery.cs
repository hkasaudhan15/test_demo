using CleanArch.Domain.Primitives.Results;
using MediatR;

namespace CleanArch.Application.Abstractions.Messaging.Queries;

/// <summary>
/// Marker interface for CQRS Queries — read-only, return Result.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Query handler — processes a query and returns Result.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
