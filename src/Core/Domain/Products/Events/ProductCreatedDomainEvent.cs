using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Products.Events;

/// <summary>
/// Raised when a new product is created.
/// Captured by the OutboxInterceptor → serialized to OutboxMessage →
/// published by OutboxProcessor → handled by ProductCreatedDomainEventHandler.
/// </summary>
public sealed record ProductCreatedDomainEvent(Guid ProductId, string Name, decimal Price) : DomainEvent;
