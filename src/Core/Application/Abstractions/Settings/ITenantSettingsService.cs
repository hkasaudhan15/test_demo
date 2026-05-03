namespace CleanArch.Application.Abstractions.Settings;

/// <summary>
/// Tenant-aware settings service — resolves effective settings using cascade:
///   1. TenantSetting (per-tenant override) → if found, return
///   2. GlobalSetting (system default) → fallback
///
/// Usage:
///   var maxAttempts = await _tenantSettings.GetEffectiveAsync&lt;int&gt;("tenant-1", SettingKeys.Security.MaxLoginAttempts);
///   // Returns tenant override if exists, otherwise global default
///
///   var allEmail = await _tenantSettings.GetEffectiveByGroupAsync("tenant-1", "Email");
///   // Returns merged dictionary: global defaults overridden by tenant-specific values
/// </summary>
public interface ITenantSettingsService
{
    Task<T> GetEffectiveAsync<T>(string tenantId, string key, CancellationToken ct = default) where T : IParsable<T>;
    Task<T> GetEffectiveOrDefaultAsync<T>(string tenantId, string key, T defaultValue, CancellationToken ct = default) where T : IParsable<T>;
    Task<string?> GetEffectiveRawAsync(string tenantId, string key, CancellationToken ct = default);
    Task<Dictionary<string, EffectiveSetting>> GetEffectiveByGroupAsync(string tenantId, string group, CancellationToken ct = default);
    Task<Dictionary<string, EffectiveSetting>> GetAllEffectiveAsync(string tenantId, CancellationToken ct = default);
    Task InvalidateTenantCacheAsync(string tenantId, string key, CancellationToken ct = default);
    Task InvalidateTenantGroupCacheAsync(string tenantId, string group, CancellationToken ct = default);
}

/// <summary>
/// Represents a resolved setting with its source (Global or Tenant override).
/// Useful for admin panels to show which settings have been customized.
/// </summary>
public sealed record EffectiveSetting(
    string Key,
    string Value,
    string Group,
    SettingSource Source,
    string? Description);

public enum SettingSource
{
    Global = 0,
    TenantOverride = 1
}
