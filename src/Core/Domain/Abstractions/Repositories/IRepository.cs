using System.Linq.Expressions;
using CleanArch.Domain.Abstractions.Entities;
using CleanArch.Domain.Specifications;

namespace CleanArch.Domain.Abstractions.Repositories;

/// <summary>
/// Generic repository contract. Only aggregate roots should have repositories.
/// Infrastructure layer implements this using EF Core or Dapper.
/// </summary>
/// <typeparam name="TEntity">Must be an aggregate root</typeparam>
public interface IRepository<TEntity> where TEntity : AggregateRoot
{
    // ─── Read ───────────────────────────────────────────
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> FindAsync(Specification<TEntity> specification, CancellationToken ct = default);
    Task<TEntity?> FirstOrDefaultAsync(Specification<TEntity> specification, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);
    Task<int> CountAsync(Specification<TEntity> specification, CancellationToken ct = default);

    // ─── Write ──────────────────────────────────────────
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
}
