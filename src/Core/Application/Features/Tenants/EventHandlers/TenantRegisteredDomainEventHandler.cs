using CleanArch.Domain.Tenants.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Features.Tenants.EventHandlers;

public sealed class TenantRegisteredDomainEventHandler : INotificationHandler<TenantRegisteredDomainEvent>
{
    private readonly ILogger<TenantRegisteredDomainEventHandler> _logger;

    public TenantRegisteredDomainEventHandler(ILogger<TenantRegisteredDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TenantRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Tenant registered — Id={TenantId}, Identifier={Identifier}, Name={Name}, Tier={Tier}",
            notification.TenantId, notification.Identifier, notification.Name, notification.Tier);

        return Task.CompletedTask;
    }
}
