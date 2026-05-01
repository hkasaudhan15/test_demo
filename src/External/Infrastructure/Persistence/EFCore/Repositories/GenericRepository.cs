using System.Linq.Expressions;
using CleanArch.Domain.Abstractions.Entities;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Specifications;
using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Persistence.EFCore.Repositories;

/// <summary>
/// Generic EF Core repository implementation.
/// Read operations use AsNoTracking for performance.
/// Supports the Specification pattern for complex queries with
/// includes, ordering, and server-side paging.
/// </summary>
public class GenericRepository<TEntity> : IRepository<TEntity>
    where TEntity : AggregateRoot
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    protected ApplicationDbContext Context => _context;
    protected DbSet<TEntity> DbSet => _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync([id], cancellationToken: ct);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Specification<TEntity> specification,
        CancellationToken ct = default)
    {
        return await ApplySpecification(specification).ToListAsync(ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Specification<TEntity> specification,
        CancellationToken ct = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(predicate, ct);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        return predicate is null
            ? await DbSet.AsNoTracking().CountAsync(ct)
            : await DbSet.AsNoTracking().CountAsync(predicate, ct);
    }

    public async Task<int> CountAsync(
        Specification<TEntity> specification,
        CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking()
            .Where(specification.ToExpression())
            .CountAsync(ct);
    }

    public void Add(TEntity entity) => DbSet.Add(entity);
    public void AddRange(IEnumerable<TEntity> entities) => DbSet.AddRange(entities);
    public void Update(TEntity entity) => DbSet.Update(entity);
    public void Remove(TEntity entity) => DbSet.Remove(entity);
    public void RemoveRange(IEnumerable<TEntity> entities) => DbSet.RemoveRange(entities);

    /// <summary>
    /// Applies a Specification to a queryable: filter → includes → ordering → paging.
    /// </summary>
    private IQueryable<TEntity> ApplySpecification(Specification<TEntity> specification)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        // 1. Filter
        query = query.Where(specification.ToExpression());

        // 2. Eager-load includes
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // 3. Ordering
        if (specification.OrderByExpression is not null)
        {
            query = query.OrderBy(specification.OrderByExpression);
        }
        else if (specification.OrderByDescendingExpression is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescendingExpression);
        }

        // 4. Paging
        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        return query;
    }
}
