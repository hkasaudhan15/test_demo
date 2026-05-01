using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Abstractions.Behaviors;

/// <summary>
/// MediatR pipeline behavior: logs warnings for slow requests (>500ms).
/// Critical for identifying performance bottlenecks in production.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int SlowRequestThresholdMs = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "⚠️ Slow Request: {RequestName} took {ElapsedMs}ms | {@Request}",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds,
                request);
        }

        return response;
    }
}
