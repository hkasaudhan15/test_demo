using CleanArch.CrossCutting.MultiTenancy.Abstractions;

namespace CleanArch.CrossCutting.MultiTenancy;

/// <summary>
/// Scoped service holding the resolved tenant for the current request.
/// Populated by <see cref="TenantResolutionMiddleware"/> early in the pipeline.
/// Injected into interceptors, query filters, and application services.
/// </summary>
public sealed class TenantProvider : ITenantProvider
{
    public string? TenantId { get; private set; }
    public string? TenantName { get; private set; }
    public string? ConnectionString { get; private set; }

    internal void SetTenant(TenantInfo tenant)
    {
        TenantId = tenant.TenantId;
        TenantName = tenant.TenantName;
        ConnectionString = tenant.ConnectionString;
    }

    internal void Clear()
    {
        TenantId = null;
        TenantName = null;
        ConnectionString = null;
    }
}
