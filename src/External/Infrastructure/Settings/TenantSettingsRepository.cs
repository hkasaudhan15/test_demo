using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Settings;
using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Settings;

public sealed class TenantSettingsRepository : ITenantSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public TenantSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantSetting?> GetByKeyAsync(string tenantId, string key, CancellationToken ct = default)
    {
        return await _context.Set<TenantSetting>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Key == key, ct);
    }

    public async Task<IReadOnlyList<TenantSetting>> GetByGroupAsync(
        string tenantId, string group, CancellationToken ct = default)
    {
        return await _context.Set<TenantSetting>()
            .Where(s => s.TenantId == tenantId && s.Group == group)
            .OrderBy(s => s.Key)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TenantSetting>> GetAllForTenantAsync(
        string tenantId, CancellationToken ct = default)
    {
        return await _context.Set<TenantSetting>()
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.Key)
            .ToListAsync(ct);
    }

    public void Add(TenantSetting setting) => _context.Set<TenantSetting>().Add(setting);
    public void Update(TenantSetting setting) => _context.Set<TenantSetting>().Update(setting);
    public void Remove(TenantSetting setting) => _context.Set<TenantSetting>().Remove(setting);
    public void RemoveRange(IEnumerable<TenantSetting> settings) => _context.Set<TenantSetting>().RemoveRange(settings);
}
