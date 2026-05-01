using CleanArch.Application.Abstractions.Caching;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Settings;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Settings;

/// <summary>
/// Tenant-aware settings service with cascading resolution and caching.
///
/// Resolution order:
///   1. Check tenant-scoped cache → "tenant-settings:{tenantId}:{key}"
///   2. Check DB TenantSettings for (TenantId, Key)
///   3. Fall back to GlobalSettings for (Key)
///
/// Cache keys:
///   - "tenant-settings:{tenantId}:{key}" for individual settings
///   - "tenant-settings:{tenantId}:group:{group}" for group listings
///   - "tenant-settings:{tenantId}:all" for full effective listing
/// </summary>
public sealed class CachedTenantSettingsService : ITenantSettingsService
{
    private const string Prefix = "tenant-settings:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private readonly ITenantSettingsRepository _tenantRepo;
    private readonly ISettingsRepository _globalRepo;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedTenantSettingsService> _logger;

    public CachedTenantSettingsService(
        ITenantSettingsRepository tenantRepo,
        ISettingsRepository globalRepo,
        ICacheService cache,
        ILogger<CachedTenantSettingsService> logger)
    {
        _tenantRepo = tenantRepo;
        _globalRepo = globalRepo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> GetEffectiveAsync<T>(string tenantId, string key, CancellationToken ct = default)
        where T : IParsable<T>
    {
        var raw = await GetEffectiveRawAsync(tenantId, key, ct)
            ?? throw new KeyNotFoundException($"Setting '{key}' not found for tenant '{tenantId}' or globally.");

        return T.Parse(raw, null);
    }

    public async Task<T> GetEffectiveOrDefaultAsync<T>(
        string tenantId, string key, T defaultValue, CancellationToken ct = default)
        where T : IParsable<T>
    {
        var raw = await GetEffectiveRawAsync(tenantId, key, ct);
        if (raw is null) return defaultValue;
        return T.TryParse(raw, null, out var result) ? result : defaultValue;
    }

    public async Task<string?> GetEffectiveRawAsync(string tenantId, string key, CancellationToken ct = default)
    {
        var cacheKey = $"{Prefix}{tenantId}:{key}";

        var cached = await _cache.GetAsync<string>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var tenantSetting = await _tenantRepo.GetByKeyAsync(tenantId, key, ct);
        if (tenantSetting is not null)
        {
            await _cache.SetAsync(cacheKey, tenantSetting.Value, CacheDuration, ct);
            return tenantSetting.Value;
        }

        var globalSetting = await _globalRepo.GetByKeyAsync(key, ct);
        if (globalSetting is not null)
        {
            await _cache.SetAsync(cacheKey, globalSetting.Value, CacheDuration, ct);
            return globalSetting.Value;
        }

        return null;
    }

    public async Task<Dictionary<string, EffectiveSetting>> GetEffectiveByGroupAsync(
        string tenantId, string group, CancellationToken ct = default)
    {
        var cacheKey = $"{Prefix}{tenantId}:group:{group}";

        var cached = await _cache.GetAsync<Dictionary<string, EffectiveSetting>>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var result = new Dictionary<string, EffectiveSetting>();

        var globalSettings = await _globalRepo.GetByGroupAsync(group, ct);
        foreach (var gs in globalSettings)
        {
            result[gs.Key] = new EffectiveSetting(
                gs.Key, gs.Value, gs.Group, SettingSource.Global, gs.Description);
        }

        var tenantOverrides = await _tenantRepo.GetByGroupAsync(tenantId, group, ct);
        foreach (var ts in tenantOverrides)
        {
            result[ts.Key] = new EffectiveSetting(
                ts.Key, ts.Value, ts.Group, SettingSource.TenantOverride, ts.Description);
        }

        await _cache.SetAsync(cacheKey, result, CacheDuration, ct);
        return result;
    }

    public async Task<Dictionary<string, EffectiveSetting>> GetAllEffectiveAsync(
        string tenantId, CancellationToken ct = default)
    {
        var cacheKey = $"{Prefix}{tenantId}:all";

        var cached = await _cache.GetAsync<Dictionary<string, EffectiveSetting>>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var result = new Dictionary<string, EffectiveSetting>();

        var globalSettings = await _globalRepo.GetAllAsync(ct);
        foreach (var gs in globalSettings)
        {
            result[gs.Key] = new EffectiveSetting(
                gs.Key, gs.Value, gs.Group, SettingSource.Global, gs.Description);
        }

        var tenantOverrides = await _tenantRepo.GetAllForTenantAsync(tenantId, ct);
        foreach (var ts in tenantOverrides)
        {
            result[ts.Key] = new EffectiveSetting(
                ts.Key, ts.Value, ts.Group, SettingSource.TenantOverride, ts.Description);
        }

        await _cache.SetAsync(cacheKey, result, CacheDuration, ct);
        return result;
    }

    public async Task InvalidateTenantCacheAsync(string tenantId, string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync($"{Prefix}{tenantId}:{key}", ct);
        await _cache.RemoveAsync($"{Prefix}{tenantId}:all", ct);
        _logger.LogDebug("Tenant setting cache invalidated — Tenant={TenantId}, Key={Key}", tenantId, key);
    }

    public async Task InvalidateTenantGroupCacheAsync(string tenantId, string group, CancellationToken ct = default)
    {
        await _cache.RemoveAsync($"{Prefix}{tenantId}:group:{group}", ct);
        await _cache.RemoveAsync($"{Prefix}{tenantId}:all", ct);
        _logger.LogDebug("Tenant setting group cache invalidated — Tenant={TenantId}, Group={Group}", tenantId, group);
    }
}
