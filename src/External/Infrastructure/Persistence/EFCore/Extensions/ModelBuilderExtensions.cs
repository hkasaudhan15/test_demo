using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Persistence.EFCore.Extensions;

/// <summary>
/// Convention-based model configuration applied automatically to every entity
/// that implements the domain interfaces (<see cref="IAuditableEntity"/>,
/// <see cref="ISoftDeletable"/>, <see cref="IHasDomainEvents"/>).
///
/// This removes the need to manually configure audit/soft-delete columns
/// in every entity's <c>IEntityTypeConfiguration</c> and ensures consistency
/// across the entire schema.
/// </summary>
public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyDomainConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // ── Auditable Entity convention ──────────────
            if (typeof(IAuditableEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType, builder =>
                {
                    builder.Property(nameof(IAuditableEntity.CreatedOnUtc))
                        .IsRequired();

                    builder.Property(nameof(IAuditableEntity.CreatedBy))
                        .HasMaxLength(256);

                    builder.Property(nameof(IAuditableEntity.ModifiedOnUtc));

                    builder.Property(nameof(IAuditableEntity.ModifiedBy))
                        .HasMaxLength(256);
                });
            }

            // ── Soft-Deletable convention ────────────────
            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType, builder =>
                {
                    builder.Property(nameof(ISoftDeletable.IsDeleted))
                        .IsRequired()
                        .HasDefaultValue(false);

                    builder.Property(nameof(ISoftDeletable.DeletedOnUtc));

                    builder.Property(nameof(ISoftDeletable.DeletedBy))
                        .HasMaxLength(256);

                    // Global query filter — automatically exclude soft-deleted rows
                    builder.HasQueryFilter(GenerateSoftDeleteFilter(clrType));
                });
            }

            // ── Domain Events (transient — never mapped) ─
            if (typeof(IHasDomainEvents).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType)
                    .Ignore(nameof(IHasDomainEvents.DomainEvents));
            }

            // ── AggregateRoot concurrency token ──────────
            if (typeof(AggregateRoot).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType)
                    .Property(nameof(AggregateRoot.RowVersion))
                    .IsRowVersion();
            }
        }

        return modelBuilder;
    }

    private static System.Linq.Expressions.LambdaExpression GenerateSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(
            property,
            System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }
}
