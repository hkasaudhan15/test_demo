using System.Linq.Expressions;
using CleanArch.CrossCutting.MultiTenancy.Abstractions;
using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Persistence.EFCore.Extensions;

/// <summary>
/// Convention-based model configuration applied automatically to every entity
/// that implements the domain interfaces (<see cref="IAuditableEntity"/>,
/// <see cref="ISoftDeletable"/>, <see cref="IHasDomainEvents"/>, <see cref="ITenantEntity"/>).
///
/// This removes the need to manually configure audit/soft-delete/tenant columns
/// in every entity's <c>IEntityTypeConfiguration</c> and ensures consistency
/// across the entire schema.
/// </summary>
public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyDomainConventions(this ModelBuilder modelBuilder, ITenantProvider tenantProvider)
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

            // ── Tenant isolation convention ──────────────
            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType, builder =>
                {
                    builder.Property(nameof(ITenantEntity.TenantId))
                        .IsRequired()
                        .HasMaxLength(64);

                    builder.HasIndex(nameof(ITenantEntity.TenantId))
                        .HasDatabaseName($"IX_{entityType.GetTableName()}_TenantId");

                    // Global query filter — isolate tenants automatically.
                    // Combined with soft-delete filter if both interfaces are present.
                    var existingFilter = builder.Metadata.GetQueryFilter();
                    var tenantFilter = GenerateTenantFilter(clrType, tenantProvider);

                    if (existingFilter is not null)
                    {
                        builder.HasQueryFilter(CombineFilters(clrType, existingFilter, tenantFilter));
                    }
                    else
                    {
                        builder.HasQueryFilter(tenantFilter);
                    }
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

    private static LambdaExpression GenerateSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    private static LambdaExpression GenerateTenantFilter(Type entityType, ITenantProvider tenantProvider)
    {
        // e => e.TenantId == tenantProvider.TenantId
        var parameter = Expression.Parameter(entityType, "e");
        var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));

        // Access tenantProvider.TenantId as a member expression (not constant)
        // so EF Core re-evaluates it per query scope.
        var providerConstant = Expression.Constant(tenantProvider);
        var providerTenantId = Expression.Property(providerConstant, nameof(ITenantProvider.TenantId));

        var condition = Expression.Equal(tenantIdProperty, providerTenantId);
        return Expression.Lambda(condition, parameter);
    }

    private static LambdaExpression CombineFilters(Type entityType, LambdaExpression first, LambdaExpression second)
    {
        var parameter = Expression.Parameter(entityType, "e");

        var firstBody = new ParameterReplacer(first.Parameters[0], parameter).Visit(first.Body);
        var secondBody = new ParameterReplacer(second.Parameters[0], parameter).Visit(second.Body);

        var combined = Expression.AndAlso(firstBody!, secondBody!);
        return Expression.Lambda(combined, parameter);
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _old;
        private readonly ParameterExpression _new;

        public ParameterReplacer(ParameterExpression old, ParameterExpression @new)
        {
            _old = old;
            _new = @new;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _old ? _new : base.VisitParameter(node);
    }
}
