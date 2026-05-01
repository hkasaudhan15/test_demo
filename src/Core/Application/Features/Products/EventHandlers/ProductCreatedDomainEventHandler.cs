using CleanArch.Domain.Products.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Features.Products.EventHandlers;

/// <summary>
/// Handles the ProductCreatedDomainEvent after it's published by the OutboxProcessor.
///
/// This handler is invoked asynchronously (eventually consistent) via:
///   Product.Create() → SaveChanges → OutboxInterceptor → OutboxMessage table
///   → OutboxProcessor (background) → MediatR.Publish → this handler
///
/// Typical use cases:
///   - Send notification email to product team
///   - Update search index (Elasticsearch)
///   - Publish to external event bus (RabbitMQ, Kafka)
///   - Update read model / materialized view
/// </summary>
public sealed class ProductCreatedDomainEventHandler : INotificationHandler<ProductCreatedDomainEvent>
{
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger;

    public ProductCreatedDomainEventHandler(ILogger<ProductCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain event handled: Product created — Id={ProductId}, Name={Name}, Price={Price}",
            notification.ProductId,
            notification.Name,
            notification.Price);

        // TODO: Add real side effects here:
        // await _emailService.SendProductCreatedNotificationAsync(notification.ProductId);
        // await _searchIndex.IndexProductAsync(notification.ProductId);
        // await _eventBus.PublishAsync(new ProductCreatedIntegrationEvent(...));

        return Task.CompletedTask;
    }
}
