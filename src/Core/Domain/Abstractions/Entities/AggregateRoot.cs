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
    /// Version for optimistic concurrency control.
    /// Incremented on every save via EF Core interceptor.
    /// </summary>
    public int Version { get; set; }
}
