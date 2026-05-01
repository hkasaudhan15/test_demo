namespace CleanArch.Domain.Abstractions.Entities;

/// <summary>
/// Marker interface for aggregate roots.
/// Only aggregate roots should have repositories.
/// Enforces DDD aggregate boundary pattern.
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id) { }

    protected AggregateRoot() { } // EF Core

    /// <summary>
    /// Row version for optimistic concurrency control.
    /// Configured as a SQL Server rowversion/timestamp column
    /// that is automatically updated by the database on every write.
    /// </summary>
    public byte[] RowVersion { get; private set; } = [];
}
