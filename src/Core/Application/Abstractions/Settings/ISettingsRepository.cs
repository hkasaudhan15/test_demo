using CleanArch.Domain.Settings;

namespace CleanArch.Application.Abstractions.Settings;

/// <summary>
/// Repository for GlobalSetting entities.
/// Separate from IRepository&lt;T&gt; because GlobalSetting is not an AggregateRoot.
/// </summary>
public interface ISettingsRepository
{
    Task<GlobalSetting?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<IReadOnlyList<GlobalSetting>> GetByGroupAsync(string group, CancellationToken ct = default);
    Task<IReadOnlyList<GlobalSetting>> GetAllAsync(CancellationToken ct = default);
    void Add(GlobalSetting setting);
    void Update(GlobalSetting setting);
    void Remove(GlobalSetting setting);
}
