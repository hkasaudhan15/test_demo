using CleanArch.CrossCutting.MultiTenancy.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArch.CrossCutting.MultiTenancy;

/// <summary>
/// Middleware that resolves the tenant for each incoming request and populates
/// the scoped <see cref="TenantProvider"/>.
///
/// Place this early in the pipeline — after CorrelationId, before authentication.
/// If the tenant cannot be resolved, the request continues without tenant context
/// (suitable for public endpoints, health checks, etc.). Use RequireTenant attribute
/// or a pipeline behavior to enforce tenancy on specific endpoints.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var resolver = context.RequestServices.GetRequiredService<ITenantResolver>();
        var tenantInfo = await resolver.ResolveAsync(context);

        if (tenantInfo is not null)
        {
            if (!tenantInfo.IsActive)
            {
                _logger.LogWarning("Request from inactive tenant {TenantId}", tenantInfo.TenantId);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Tenant is inactive." });
                return;
            }

            var provider = context.RequestServices.GetRequiredService<TenantProvider>();
            provider.SetTenant(tenantInfo);

            // TenantId is added to structured logs via the RequestLoggingEnricher
            // which already reads X-Tenant-Id from the request headers.
            _logger.LogDebug("Resolved tenant {TenantId}", tenantInfo.TenantId);
        }

        await _next(context);
    }
}
