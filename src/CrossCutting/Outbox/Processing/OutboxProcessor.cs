using System.Text.Json;
using CleanArch.CrossCutting.Outbox.Abstractions;
using CleanArch.Domain.Abstractions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArch.CrossCutting.Outbox.Processing;

/// <summary>
/// Background service that reliably processes outbox messages.
///
/// Design decisions (industry-standard):
///   - Per-message SaveChanges: if the process crashes mid-batch, only
///     the current message is reprocessed — no duplicates for others.
///   - Exponential backoff: transient failures are retried with increasing
///     delays (via <see cref="OutboxMessage.RecordFailure"/>).
///   - Dead-lettering: messages exceeding <see cref="OutboxOptions.MaxRetries"/>
///     are marked <see cref="OutboxMessageStatus.Failed"/>.
///   - Configuration via <see cref="OutboxOptions"/> bound from appsettings.json.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox processor started (interval={Interval}s, batch={Batch}, maxRetries={Retries})",
            _options.IntervalSeconds, _options.BatchSize, _options.MaxRetries);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in outbox processor loop");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var utcNow = DateTime.UtcNow;

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .Where(m => m.NextRetryOnUtc == null || m.NextRetryOnUtc <= utcNow)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(_options.BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            await ProcessSingleMessageAsync(message, publisher, dbContext, ct);
        }
    }

    private async Task ProcessSingleMessageAsync(
        OutboxMessage message,
        IPublisher publisher,
        DbContext dbContext,
        CancellationToken ct)
    {
        try
        {
            var eventType = Type.GetType(message.EventType);
            if (eventType is null)
            {
                _logger.LogWarning(
                    "Unknown event type {EventType} for message {MessageId}. Dead-lettering.",
                    message.EventType, message.Id);

                message.RecordFailure(
                    $"Unknown event type: {message.EventType}",
                    DateTime.UtcNow,
                    maxRetries: 1); // immediate dead-letter

                await dbContext.SaveChangesAsync(ct);
                return;
            }

            var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);

            if (domainEvent is not IDomainEvent typedEvent)
            {
                _logger.LogWarning(
                    "Failed to deserialize message {MessageId} as IDomainEvent. Dead-lettering.",
                    message.Id);

                message.RecordFailure(
                    "Deserialized object does not implement IDomainEvent",
                    DateTime.UtcNow,
                    maxRetries: 1);

                await dbContext.SaveChangesAsync(ct);
                return;
            }

            await publisher.Publish(typedEvent, ct);

            message.MarkAsProcessed(DateTime.UtcNow);
            await dbContext.SaveChangesAsync(ct);

            _logger.LogDebug(
                "Processed outbox message {MessageId} ({EventType})",
                message.Id, message.EventType);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            message.RecordFailure(ex.ToString(), DateTime.UtcNow, _options.MaxRetries);
            await dbContext.SaveChangesAsync(ct);

            if (message.Status == OutboxMessageStatus.Failed)
            {
                _logger.LogError(ex,
                    "Outbox message {MessageId} dead-lettered after {RetryCount} retries",
                    message.Id, message.RetryCount);
            }
            else
            {
                _logger.LogWarning(ex,
                    "Outbox message {MessageId} failed (retry {RetryCount}/{MaxRetries}, next retry at {NextRetry})",
                    message.Id, message.RetryCount, _options.MaxRetries, message.NextRetryOnUtc);
            }
        }
    }
}
