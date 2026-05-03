using CleanArch.Domain.Tenants.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Features.Tenants.EventHandlers;

public sealed class TenantSubscriptionChangedDomainEventHandler
    : INotificationHandler<TenantSubscriptionChangedDomainEvent>
{
    private readonly ILogger<TenantSubscriptionChangedDomainEventHandler> _logger;

    public TenantSubscriptionChangedDomainEventHandler(
        ILogger<TenantSubscriptionChangedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TenantSubscriptionChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Tenant subscription changed — TenantId={TenantId}, {OldTier} → {NewTier}",
            notification.TenantId, notification.OldTier, notification.NewTier);

        return Task.CompletedTask;
    }
}
