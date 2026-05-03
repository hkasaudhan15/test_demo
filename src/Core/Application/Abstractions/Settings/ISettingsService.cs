namespace CleanArch.Application.Abstractions.Settings;

/// <summary>
/// Type-safe settings service abstraction. Reads from DB with cache-aside pattern.
/// Infrastructure layer implements this with EF Core + ICacheService.
///
/// Usage:
///   var appName = await _settings.GetAsync&lt;string&gt;(SettingKeys.General.ApplicationName);
///   var maxAttempts = await _settings.GetAsync&lt;int&gt;(SettingKeys.Security.MaxLoginAttempts);
///   var maintenanceMode = await _settings.GetAsync&lt;bool&gt;(SettingKeys.General.MaintenanceMode);
/// </summary>
public interface ISettingsService
{
    Task<T> GetAsync<T>(string key, CancellationToken ct = default) where T : IParsable<T>;
    Task<T> GetOrDefaultAsync<T>(string key, T defaultValue, CancellationToken ct = default) where T : IParsable<T>;
    Task<string?> GetRawAsync(string key, CancellationToken ct = default);
    Task<Dictionary<string, string>> GetByGroupAsync(string group, CancellationToken ct = default);
    Task InvalidateCacheAsync(string key, CancellationToken ct = default);
    Task InvalidateGroupCacheAsync(string group, CancellationToken ct = default);
}
