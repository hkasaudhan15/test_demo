using MediatR;

namespace CleanArch.Domain.Abstractions.Events;

/// <summary>
/// Marker interface for domain events.
/// Events are dispatched after SaveChanges via MediatR.
/// Extends INotification so MediatR can publish them.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}

/// <summary>
/// Base record for domain events — immutable by design.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
