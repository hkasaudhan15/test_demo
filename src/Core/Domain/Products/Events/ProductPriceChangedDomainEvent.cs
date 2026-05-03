using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Products.Events;

/// <summary>
/// Raised when a product's price is updated.
/// Example use case: notify subscribed customers, update search index, audit trail.
/// </summary>
public sealed record ProductPriceChangedDomainEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice) : DomainEvent;
