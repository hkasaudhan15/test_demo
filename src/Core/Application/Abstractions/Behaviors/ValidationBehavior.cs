using CleanArch.Domain.Primitives.Results;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace CleanArch.Application.Abstractions.Behaviors;

/// <summary>
/// MediatR pipeline behavior: runs ALL FluentValidation validators
/// before the handler executes. Returns ValidationError on failure.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => new Error(
                f.ErrorCode ?? $"Validation.{f.PropertyName}",
                f.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Length != 0)
        {
            return CreateValidationResult<TResponse>(new ValidationError(errors));
        }

        return await next();
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, MethodInfo> FailureMethodCache = new();

    private static TResult CreateValidationResult<TResult>(ValidationError error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
            return (Result.Failure(error) as TResult)!;

        var resultType = typeof(TResult).GenericTypeArguments[0];
        
        var failureMethod = FailureMethodCache.GetOrAdd(resultType, type => 
            typeof(Result)
                .GetMethods()
                .First(m => m.Name == "Failure" && m.IsGenericMethod)
                .MakeGenericMethod(type));

        return (TResult)failureMethod.Invoke(null, [error])!;
    }
}
