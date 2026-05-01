using System.Text.Json;
using CleanArch.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Caching.Redis;

/// <summary>
/// Redis-backed distributed cache implementation with graceful degradation.
/// All operations swallow cache failures so the application continues
/// to function (albeit without caching) when Redis is unavailable.
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
            _logger.LogWarning(ex, "Redis GET failed for key {Key}. Falling back to source.", key);
            return default;
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
            _logger.LogWarning(ex, "Redis SET failed for key {Key}. Continuing without cache.", key);
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
            _logger.LogWarning(ex, "Redis REMOVE failed for key {Key}.", key);
        }
    }

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken ct = default)
    {
        // IDistributedCache does not expose key-scanning.
        // For production use with prefix-based invalidation, inject
        // IConnectionMultiplexer directly and use SCAN + UNLINK.
        // Logging the no-op so callers are aware.
        _logger.LogWarning(
            "RemoveByPrefixAsync is a no-op with IDistributedCache. " +
            "Inject IConnectionMultiplexer for prefix-based invalidation. Prefix: {Prefix}",
            prefixKey);

        return Task.CompletedTask;
    }
}
