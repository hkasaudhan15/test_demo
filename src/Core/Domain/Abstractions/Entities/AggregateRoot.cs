using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Abstractions.Entities;

/// <summary>
/// Aggregate root — the DDD consistency boundary.
/// Only aggregate roots should have repositories.
///
/// Composes all standard concerns:
///   - <see cref="IAuditableEntity"/>  — creation/modification timestamps
///   - <see cref="ISoftDeletable"/>    — soft-delete instead of physical delete
///   - <see cref="IHasDomainEvents"/>  — domain event collection and dispatch
///   - Optimistic concurrency via <see cref="RowVersion"/>
///
/// If a child entity does not need all of these, it should extend
/// <see cref="Entity"/> directly and opt into only the interfaces it needs.
/// </summary>
public abstract class AggregateRoot : Entity, IAuditableEntity, ISoftDeletable, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(Guid id) : base(id) { }

    protected AggregateRoot() { } // EF Core

    // ─── IAuditableEntity ────────────────────────────────
    public DateTime CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ─── ISoftDeletable ──────────────────────────────────
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }

    // ─── IHasDomainEvents ────────────────────────────────
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // ─── Optimistic Concurrency ──────────────────────────
    /// <summary>
    /// SQL Server rowversion/timestamp — automatically incremented by the
    /// database engine on every write. EF Core checks this value on
    /// UPDATE/DELETE and throws <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/>
    /// on mismatch.
    /// </summary>
    public byte[] RowVersion { get; private set; } = [];
}
