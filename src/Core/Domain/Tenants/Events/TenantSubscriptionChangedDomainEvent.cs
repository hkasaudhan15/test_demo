using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Tenants.Events;

public sealed record TenantSubscriptionChangedDomainEvent(
    Guid TenantId,
    SubscriptionTier OldTier,
    SubscriptionTier NewTier) : DomainEvent;
