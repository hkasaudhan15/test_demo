using Microsoft.AspNetCore.Http;

namespace CleanArch.CrossCutting.MultiTenancy.Abstractions;

/// <summary>
/// Provides current tenant context. Resolved per-request via middleware.
/// </summary>
public interface ITenantProvider
{
    string? TenantId { get; }
    string? TenantName { get; }
    string? ConnectionString { get; }
}

/// <summary>
/// Resolves tenant info from the incoming request.
/// Strategies: Header, Subdomain, Route, Query, Claim.
/// </summary>
public interface ITenantResolver
{
    Task<TenantInfo?> ResolveAsync(HttpContext context);
}

/// <summary>
/// Tenant information record.
/// </summary>
public sealed record TenantInfo(
    string TenantId,
    string TenantName,
    string? ConnectionString = null,
    bool IsActive = true);

