using CleanArch.Domain.Settings;

namespace CleanArch.Application.Abstractions.Settings;

/// <summary>
/// Repository for per-tenant setting overrides.
/// </summary>
public interface ITenantSettingsRepository
{
    Task<TenantSetting?> GetByKeyAsync(string tenantId, string key, CancellationToken ct = default);
    Task<IReadOnlyList<TenantSetting>> GetByGroupAsync(string tenantId, string group, CancellationToken ct = default);
    Task<IReadOnlyList<TenantSetting>> GetAllForTenantAsync(string tenantId, CancellationToken ct = default);
    void Add(TenantSetting setting);
    void Update(TenantSetting setting);
    void Remove(TenantSetting setting);
    void RemoveRange(IEnumerable<TenantSetting> settings);
}
