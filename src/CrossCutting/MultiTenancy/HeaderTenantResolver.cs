using CleanArch.CrossCutting.MultiTenancy.Abstractions;
using CleanArch.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleanArch.CrossCutting.MultiTenancy;

/// <summary>
/// Resolves tenant from the X-Tenant-Id request header, backed by DB lookup with caching.
///
/// Resolution flow:
///   1. Read X-Tenant-Id header
///   2. Check distributed cache for tenant info
///   3. Cache miss → delegate to ITenantStore for DB lookup
///   4. Cache hit → deserialize and return
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
    private const string CachePrefix = "tenant-resolution:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private readonly ITenantStore _tenantStore;
    private readonly IDistributedCache _cache;
    private readonly ILogger<HeaderTenantResolver> _logger;

    public HeaderTenantResolver(
        ITenantStore tenantStore,
        IDistributedCache cache,
        ILogger<HeaderTenantResolver> logger)
    {
        _tenantStore = tenantStore;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TenantInfo?> ResolveAsync(HttpContext context)
    {
        var tenantId = context.Request.Headers[AppConstants.Headers.TenantId].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantId))
            return null;

        var cacheKey = $"{CachePrefix}{tenantId}";

        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<TenantInfo>(cached);
        }

        var tenantInfo = await _tenantStore.GetByIdentifierAsync(tenantId);
        if (tenantInfo is null)
        {
            _logger.LogWarning("Tenant resolution failed — identifier '{TenantId}' not found in store", tenantId);
            return null;
        }

        var serialized = JsonSerializer.Serialize(tenantInfo);
        await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        });

        return tenantInfo;
    }
}
