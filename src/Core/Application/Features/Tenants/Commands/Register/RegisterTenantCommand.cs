using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Commands.Register;

public sealed record RegisterTenantCommand(
    string Identifier,
    string Name,
    string ContactEmail,
    string? AdminName = null,
    string? Description = null,
    SubscriptionTier Tier = SubscriptionTier.Free,
    bool AutoActivate = true) : ICommand<Guid>;
