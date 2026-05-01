using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Clock;
using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArch.Infrastructure.Persistence.EFCore.Interceptors;

/// <summary>
/// Automatically stamps audit fields on any entity implementing
/// <see cref="IAuditableEntity"/>. Targets the interface — not a concrete
/// base class — so entities at any level of the hierarchy can opt in.
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditableEntityInterceptor(
        ICurrentUserService currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        StampAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void StampAuditFields(DbContext? context)
    {
        if (context is null) return;

        var utcNow = _dateTimeProvider.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                SetProperty(entry, nameof(IAuditableEntity.CreatedOnUtc), utcNow);
                SetProperty(entry, nameof(IAuditableEntity.CreatedBy), userId);
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                SetProperty(entry, nameof(IAuditableEntity.ModifiedOnUtc), utcNow);
                SetProperty(entry, nameof(IAuditableEntity.ModifiedBy), userId);
            }
        }
    }

    private static void SetProperty<TValue>(EntityEntry entry, string propertyName, TValue value)
    {
        entry.Property(propertyName).CurrentValue = value;
    }
}
