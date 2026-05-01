using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

/// <summary>
/// Base configuration for concrete aggregate root entities.
///
/// Common cross-cutting properties (audit fields, soft-delete, row version,
/// domain events) are handled automatically by the convention layer
/// (<c>ModelBuilderExtensions.ApplyDomainConventions</c>).
///
/// Subclasses only need to configure table-specific columns, relationships,
/// and indexes. Override <see cref="Configure"/> and call <c>base.Configure(builder)</c>
/// to keep the primary key mapping.
/// </summary>
public abstract class AggregateRootConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AggregateRoot
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
