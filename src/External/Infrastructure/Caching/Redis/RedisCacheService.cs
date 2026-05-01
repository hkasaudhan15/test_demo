using System.Text.Json;
using CleanArch.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Caching.Redis;

/// <summary>
/// Redis-backed distributed cache implementation with graceful degradation.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var cached = await _cache.GetStringAsync(key, ct);
            return cached is null ? default : JsonSerializer.Deserialize<T>(cached, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache GET failed for key: {Key}. Falling back to source.", key);
            return default; // Graceful degradation
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _cache.SetStringAsync(key, json, options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache SET failed for key: {Key}. Continuing without cache.", key);
            // Don't throw - cache failures should not break the application
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache REMOVE failed for key: {Key}.", key);
            // Don't throw - cache failures should not break the application
        }
    }

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken ct = default)
    {
        // TODO: Implement using IConnectionMultiplexer for production use.
        // Requires direct StackExchange.Redis connection to use SCAN + DEL.
        // Example: var server = _multiplexer.GetServer(...); var keys = server.Keys(pattern: $"{prefixKey}*");
        throw new NotSupportedException(
            "Prefix-based cache removal requires IConnectionMultiplexer. " +
            "Inject IConnectionMultiplexer and implement SCAN-based key deletion.");
    }
}
