using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleanArch.Infrastructure.HealthChecks;

/// <summary>
/// Health check for Redis cache connectivity.
/// </summary>
public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IDistributedCache _cache;

    public RedisHealthCheck(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var testKey = $"health_check_{Guid.NewGuid()}";
            await _cache.SetStringAsync(testKey, "test", cancellationToken);
            var result = await _cache.GetStringAsync(testKey, cancellationToken);
            await _cache.RemoveAsync(testKey, cancellationToken);

            return result == "test"
                ? HealthCheckResult.Healthy("Redis cache is operational.")
                : HealthCheckResult.Degraded("Redis cache responded but data integrity check failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Redis cache is unreachable or not responding.",
                ex);
        }
    }
}
