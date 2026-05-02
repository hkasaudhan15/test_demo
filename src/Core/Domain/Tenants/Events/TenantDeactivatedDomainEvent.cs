using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Tenants.Events;

public sealed record TenantDeactivatedDomainEvent(Guid TenantId, string Identifier, string Reason) : DomainEvent;
