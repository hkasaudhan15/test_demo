using CleanArch.CrossCutting.MultiTenancy.Abstractions;
using CleanArch.Domain.Tenants;
using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Tenants;

/// <summary>
/// EF Core implementation of ITenantStore — looks up tenant from the Tenants table.
/// Used by HeaderTenantResolver for DB-backed tenant resolution.
/// </summary>
public sealed class EfCoreTenantStore : ITenantStore
{
    private readonly ApplicationDbContext _context;

    public EfCoreTenantStore(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken ct = default)
    {
        var tenant = await _context.Set<Tenant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Identifier == identifier, ct);

        if (tenant is null)
            return null;

        return new TenantInfo(
            TenantId: tenant.Identifier,
            TenantName: tenant.Name,
            ConnectionString: tenant.ConnectionString,
            IsActive: tenant.Status == TenantStatus.Active);
    }
}
