using System.Text.Json;
using CleanArch.CrossCutting.Outbox.Abstractions;
using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArch.CrossCutting.Outbox.Interceptors;

/// <summary>
/// EF Core interceptor that saves domain events to the outbox table
/// instead of publishing them immediately. Ensures reliable event delivery.
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await ConvertDomainEventsToOutboxMessagesAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task ConvertDomainEventsToOutboxMessagesAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        var outboxMessages = new List<OutboxMessage>();

        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().AssemblyQualifiedName!,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOnUtc = DateTime.UtcNow,
                    RetryCount = 0
                };

                outboxMessages.Add(outboxMessage);
            }

            entity.ClearDomainEvents();
        }

        if (outboxMessages.Count > 0)
        {
            await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
        }
    }
}
