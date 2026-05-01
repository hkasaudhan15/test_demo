using CleanArch.SharedKernel.Constants;

namespace CleanArch.Presentation.Api.Middleware.Correlation;

/// <summary>
/// Adds a Correlation ID to every request/response for distributed tracing.
/// If the client sends X-Correlation-Id, it's reused; otherwise auto-generated.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[AppConstants.Headers.CorrelationId].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        context.Items[AppConstants.Headers.CorrelationId] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[AppConstants.Headers.CorrelationId] = correlationId;
            return Task.CompletedTask;
        });

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
