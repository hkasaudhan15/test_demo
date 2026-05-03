using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Tenants;
using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Tenants;

public sealed class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Tenant>()
            .Include(t => t.Features)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Tenant?> GetByIdentifierAsync(string identifier, CancellationToken ct = default)
    {
        return await _context.Set<Tenant>()
            .Include(t => t.Features)
            .FirstOrDefaultAsync(t => t.Identifier == identifier, ct);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<Tenant>()
            .Include(t => t.Features)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Tenant>> GetByStatusAsync(TenantStatus status, CancellationToken ct = default)
    {
        return await _context.Set<Tenant>()
            .Include(t => t.Features)
            .Where(t => t.Status == status)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(string identifier, CancellationToken ct = default)
    {
        return await _context.Set<Tenant>()
            .AnyAsync(t => t.Identifier == identifier, ct);
    }

    public void Add(Tenant tenant) => _context.Set<Tenant>().Add(tenant);
    public void Update(Tenant tenant) => _context.Set<Tenant>().Update(tenant);
}
