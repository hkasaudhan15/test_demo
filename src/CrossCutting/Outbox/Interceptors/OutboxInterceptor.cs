using System.Text.Json;
using CleanArch.CrossCutting.Outbox.Abstractions;
using CleanArch.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArch.CrossCutting.Outbox.Interceptors;

/// <summary>
/// EF Core interceptor that converts domain events into
/// <see cref="OutboxMessage"/> records within the same transaction
/// as the aggregate save.
///
/// Intercepts <c>SavingChanges</c> (pre-save) so outbox rows participate
/// in the same commit — guaranteeing atomicity between the aggregate
/// state change and the event publication intent.
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ConvertDomainEventsToOutboxMessages(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ConvertDomainEventsToOutboxMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDomainEventsToOutboxMessages(DbContext context)
    {
        var entities = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var outboxMessages = new List<OutboxMessage>();

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                var message = OutboxMessage.Create(
                    eventType: domainEvent.GetType().AssemblyQualifiedName!,
                    payload: JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions),
                    occurredOnUtc: domainEvent.OccurredOnUtc);

                outboxMessages.Add(message);
            }

            entity.ClearDomainEvents();
        }

        if (outboxMessages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }
    }
}
