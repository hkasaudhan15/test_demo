using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Abstractions.Entities;

/// <summary>
/// Marks an entity as a source of domain events.
/// Events are collected during the business operation and dispatched
/// by the infrastructure layer after (or before) SaveChanges.
/// Typically implemented by aggregate roots only.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
