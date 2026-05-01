using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

/// <summary>
/// Base configuration for all aggregate roots.
/// Configures row version concurrency token and common audit properties.
/// </summary>
public abstract class AggregateRootConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AggregateRoot
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);

        // SQL Server rowversion — auto-incremented by the database engine on every write.
        // EF Core checks this value on UPDATE/DELETE and throws DbUpdateConcurrencyException on mismatch.
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(e => e.CreatedOnUtc)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(256);

        builder.Property(e => e.ModifiedOnUtc);

        builder.Property(e => e.ModifiedBy)
            .HasMaxLength(256);

        // Soft delete
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedOnUtc);

        builder.Property(e => e.DeletedBy)
            .HasMaxLength(256);

        // Domain events are transient — never persisted
        builder.Ignore(e => e.DomainEvents);
    }
}
