using CleanArch.Domain.Abstractions.Repositories;

namespace CleanArch.BddTests.Support;

/// <summary>
/// No-op unit of work for BDD tests.
/// In-memory repository mutations are immediate.
/// </summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }

    public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public void Dispose() { }
}
