using CleanArch.CrossCutting.MultiTenancy.Abstractions;
using CleanArch.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;

namespace CleanArch.CrossCutting.MultiTenancy;

/// <summary>
/// Resolves tenant from the X-Tenant-Id request header.
/// This is the most common strategy for API-first applications.
///
/// Alternative resolvers can be implemented for:
///   - Subdomain-based: "tenant1.api.example.com"
///   - Route-based: "/api/v1/{tenantId}/orders"
///   - Claim-based: JWT token contains tenant claim
///
/// Register the desired resolver as <see cref="ITenantResolver"/> in DI.
/// </summary>
public sealed class HeaderTenantResolver : ITenantResolver
{
    public Task<TenantInfo?> ResolveAsync(HttpContext context)
    {
        var tenantId = context.Request.Headers[AppConstants.Headers.TenantId].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantId))
            return Task.FromResult<TenantInfo?>(null);

        // In production, look up tenant from a store (DB/cache) to get
        // the tenant name, connection string, and active status.
        // For now, we trust the header and return a basic TenantInfo.
        var tenantInfo = new TenantInfo(
            TenantId: tenantId,
            TenantName: tenantId);

        return Task.FromResult<TenantInfo?>(tenantInfo);
    }
}
