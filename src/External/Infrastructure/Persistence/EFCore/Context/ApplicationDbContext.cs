using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Infrastructure.Persistence.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Persistence.EFCore.Context;

/// <summary>
/// Application DbContext — central data access point.
/// Implements <see cref="IUnitOfWork"/> for transactional consistency.
///
/// Domain conventions (audit fields, soft-delete filters, concurrency tokens,
/// domain event exclusion) are applied automatically via
/// <see cref="ModelBuilderExtensions.ApplyDomainConventions"/> so individual
/// entity configurations only need to declare their own table-specific mappings.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // ─── Register DbSets here ───────────────────────────
    // public DbSet<YourEntity> YourEntities => Set<YourEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Apply all IEntityTypeConfiguration<T> from Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // 2. Apply domain conventions (audit, soft-delete, concurrency, events)
        modelBuilder.ApplyDomainConventions();

        base.OnModelCreating(modelBuilder);
    }

    // ─── IUnitOfWork ─────────────────────────────────────

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        await Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        await Database.CommitTransactionAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        await Database.RollbackTransactionAsync(ct);
    }
}
