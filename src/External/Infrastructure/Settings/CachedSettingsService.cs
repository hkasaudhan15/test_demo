using CleanArch.Application.Abstractions.Caching;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Settings;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Settings;

/// <summary>
/// Settings service with cache-aside pattern.
/// Reads from cache first; on miss, reads from DB and populates cache.
/// Cache is invalidated on setting mutations (upsert/delete).
///
/// Cache keys:
///   - "settings:{key}" for individual settings
///   - "settings:group:{group}" for group listings
/// </summary>
public sealed class CachedSettingsService : ISettingsService
{
    private const string CachePrefix = "settings:";
    private const string GroupCachePrefix = "settings:group:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private readonly ISettingsRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedSettingsService> _logger;

    public CachedSettingsService(
        ISettingsRepository repository,
        ICacheService cache,
        ILogger<CachedSettingsService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(string key, CancellationToken ct = default) where T : IParsable<T>
    {
        var raw = await GetRawAsync(key, ct);

        if (raw is null)
            throw new KeyNotFoundException($"Setting '{key}' not found.");

        return T.Parse(raw, null);
    }

    public async Task<T> GetOrDefaultAsync<T>(string key, T defaultValue, CancellationToken ct = default)
        where T : IParsable<T>
    {
        var raw = await GetRawAsync(key, ct);

        if (raw is null)
            return defaultValue;

        return T.TryParse(raw, null, out var result) ? result : defaultValue;
    }

    public async Task<string?> GetRawAsync(string key, CancellationToken ct = default)
    {
        var cacheKey = $"{CachePrefix}{key}";

        var cached = await _cache.GetAsync<string>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var setting = await _repository.GetByKeyAsync(key, ct);
        if (setting is null)
            return null;

        await _cache.SetAsync(cacheKey, setting.Value, CacheDuration, ct);
        return setting.Value;
    }

    public async Task<Dictionary<string, string>> GetByGroupAsync(string group, CancellationToken ct = default)
    {
        var cacheKey = $"{GroupCachePrefix}{group}";

        var cached = await _cache.GetAsync<Dictionary<string, string>>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var settings = await _repository.GetByGroupAsync(group, ct);
        var dict = settings.ToDictionary(s => s.Key, s => s.Value);

        await _cache.SetAsync(cacheKey, dict, CacheDuration, ct);
        return dict;
    }

    public async Task InvalidateCacheAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync($"{CachePrefix}{key}", ct);
        _logger.LogDebug("Cache invalidated for setting {Key}", key);
    }

    public async Task InvalidateGroupCacheAsync(string group, CancellationToken ct = default)
    {
        await _cache.RemoveAsync($"{GroupCachePrefix}{group}", ct);
        _logger.LogDebug("Cache invalidated for setting group {Group}", group);
    }
}
