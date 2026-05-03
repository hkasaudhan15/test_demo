using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Commands.UpdateSubscription;

public sealed record UpdateTenantSubscriptionCommand(
    Guid TenantId,
    SubscriptionTier NewTier,
    DateTime? ExpiresOnUtc = null) : ICommand;
