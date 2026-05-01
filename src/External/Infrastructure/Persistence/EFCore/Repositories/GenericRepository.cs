using System.Linq.Expressions;
using CleanArch.Domain.Abstractions.Entities;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Persistence.EFCore.Repositories;

/// <summary>
/// Generic EF Core repository implementation.
/// Handles CRUD operations for any aggregate root.
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

    public void Add(TEntity entity) => DbSet.Add(entity);
    public void AddRange(IEnumerable<TEntity> entities) => DbSet.AddRange(entities);
    public void Update(TEntity entity) => DbSet.Update(entity);
    public void Remove(TEntity entity) => DbSet.Remove(entity);
    public void RemoveRange(IEnumerable<TEntity> entities) => DbSet.RemoveRange(entities);
}
