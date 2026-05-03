using CleanArch.Domain.Products.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Features.Products.EventHandlers;

/// <summary>
/// Handles the ProductPriceChangedDomainEvent.
/// Example: notify subscribed customers of price drops, update analytics.
/// </summary>
public sealed class ProductPriceChangedDomainEventHandler : INotificationHandler<ProductPriceChangedDomainEvent>
{
    private readonly ILogger<ProductPriceChangedDomainEventHandler> _logger;

    public ProductPriceChangedDomainEventHandler(ILogger<ProductPriceChangedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProductPriceChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain event handled: Product price changed — Id={ProductId}, OldPrice={OldPrice}, NewPrice={NewPrice}",
            notification.ProductId,
            notification.OldPrice,
            notification.NewPrice);

        // TODO: Add real side effects:
        // if (notification.NewPrice < notification.OldPrice)
        //     await _notificationService.NotifyPriceDropSubscribersAsync(notification.ProductId);

        return Task.CompletedTask;
    }
}
