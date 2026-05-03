using Serilog;

namespace CleanArch.Presentation.Api.Middleware.RequestLogging;

/// <summary>
/// Enriches Serilog request logs with extra context (user, tenant, IP).
/// </summary>
public static class RequestLoggingEnricher
{
    public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        var request = httpContext.Request;

        diagnosticContext.Set("RequestHost", request.Host.Value);
        diagnosticContext.Set("RequestScheme", request.Scheme);
        diagnosticContext.Set("QueryString", request.QueryString.HasValue ? request.QueryString.Value : string.Empty);
        diagnosticContext.Set("UserAgent", request.Headers.UserAgent.FirstOrDefault());
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        }

        var tenantId = request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (tenantId is not null)
        {
            diagnosticContext.Set("TenantId", tenantId);
        }
    }
}
