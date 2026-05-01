using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Abstractions.Behaviors;

/// <summary>
/// MediatR pipeline behavior: logs every incoming request for observability.
/// Request body is logged at Debug level only to avoid PII exposure in production.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("📥 Handling {RequestName}", requestName);

        // Only serialize request payload if Debug level is enabled
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("📥 {RequestName} payload: {@Request}", requestName, request);
        }

        var response = await next();

        _logger.LogInformation("📤 Handled {RequestName}", requestName);

        return response;
    }
}

