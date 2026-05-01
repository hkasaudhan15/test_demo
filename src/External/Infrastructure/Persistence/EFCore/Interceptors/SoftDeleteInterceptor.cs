using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Clock;
using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArch.Infrastructure.Persistence.EFCore.Interceptors;

/// <summary>
/// Converts physical DELETEs to soft deletes for any entity implementing
/// <see cref="ISoftDeletable"/>. Targets the interface — not a concrete
/// base class — so entities at any level of the hierarchy can opt in.
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SoftDeleteInterceptor(
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
        ConvertDeleteToSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        ConvertDeleteToSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void ConvertDeleteToSoftDelete(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                SetProperty(entry, nameof(ISoftDeletable.IsDeleted), true);
                SetProperty(entry, nameof(ISoftDeletable.DeletedOnUtc), _dateTimeProvider.UtcNow);
                SetProperty(entry, nameof(ISoftDeletable.DeletedBy), _currentUser.UserId);
            }
        }
    }

    private static void SetProperty<TValue>(EntityEntry entry, string propertyName, TValue value)
    {
        entry.Property(propertyName).CurrentValue = value;
    }
}
