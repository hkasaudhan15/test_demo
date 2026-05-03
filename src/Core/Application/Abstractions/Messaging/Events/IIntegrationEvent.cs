using MediatR;

namespace CleanArch.Application.Abstractions.Messaging.Events;

/// <summary>
/// Integration event — cross-boundary event for inter-service communication.
/// Differs from domain events which are intra-aggregate.
/// </summary>
public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}

/// <summary>
/// Domain event handler — handles domain events dispatched post-SaveChanges.
/// </summary>
public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : INotification
{
}
