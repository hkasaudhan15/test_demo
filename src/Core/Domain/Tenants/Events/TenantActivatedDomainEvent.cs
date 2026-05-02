using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Tenants.Events;

public sealed record TenantActivatedDomainEvent(Guid TenantId, string Identifier) : DomainEvent;
