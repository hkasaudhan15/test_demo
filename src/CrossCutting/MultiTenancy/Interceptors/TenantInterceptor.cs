using CleanArch.CrossCutting.MultiTenancy.Abstractions;
using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArch.CrossCutting.MultiTenancy.Interceptors;

/// <summary>
/// EF Core interceptor that automatically stamps TenantId on new entities
/// implementing <see cref="ITenantEntity"/>.
///
/// On insert: sets TenantId from the current <see cref="ITenantProvider"/>.
/// On update: verifies the entity still belongs to the current tenant (prevents cross-tenant data mutation).
/// </summary>
public sealed class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return ValueTask.FromResult(result);

        var currentTenantId = _tenantProvider.TenantId;

        foreach (var entry in eventData.Context.ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (!string.IsNullOrWhiteSpace(currentTenantId))
                    {
                        entry.Entity.TenantId = currentTenantId;
                    }
                    break;

                case EntityState.Modified:
                    if (!string.IsNullOrWhiteSpace(currentTenantId) &&
                        entry.Entity.TenantId != currentTenantId)
                    {
                        throw new InvalidOperationException(
                            $"Cross-tenant data modification attempted. " +
                            $"Entity tenant: '{entry.Entity.TenantId}', current tenant: '{currentTenantId}'.");
                    }
                    break;
            }
        }

        return ValueTask.FromResult(result);
    }
}
