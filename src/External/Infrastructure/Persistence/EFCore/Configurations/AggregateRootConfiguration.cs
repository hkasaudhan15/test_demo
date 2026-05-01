using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

/// <summary>
/// Base configuration for all aggregate roots.
/// Configures concurrency token and common properties.
/// </summary>
public abstract class AggregateRootConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AggregateRoot
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Concurrency token for optimistic concurrency control
        builder.Property(e => e.Version)
            .IsConcurrencyToken()
            .HasDefaultValue(0);

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

        // Ignore domain events (not persisted)
        builder.Ignore(e => e.DomainEvents);
    }
}
