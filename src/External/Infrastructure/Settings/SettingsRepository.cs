using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Settings;
using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Settings;

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly ApplicationDbContext _context;

    public SettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GlobalSetting?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        return await _context.Set<GlobalSetting>()
            .FirstOrDefaultAsync(s => s.Key == key, ct);
    }

    public async Task<IReadOnlyList<GlobalSetting>> GetByGroupAsync(string group, CancellationToken ct = default)
    {
        return await _context.Set<GlobalSetting>()
            .AsNoTracking()
            .Where(s => s.Group == group)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Key)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GlobalSetting>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<GlobalSetting>()
            .AsNoTracking()
            .OrderBy(s => s.Group)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Key)
            .ToListAsync(ct);
    }

    public void Add(GlobalSetting setting) => _context.Set<GlobalSetting>().Add(setting);
    public void Update(GlobalSetting setting) => _context.Set<GlobalSetting>().Update(setting);
    public void Remove(GlobalSetting setting) => _context.Set<GlobalSetting>().Remove(setting);
}
