using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Tenants.Events;

public sealed record TenantRegisteredDomainEvent(
    Guid TenantId,
    string Identifier,
    string Name,
    SubscriptionTier Tier) : DomainEvent;
