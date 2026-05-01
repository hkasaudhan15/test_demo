using System.Text.Json;
using CleanArch.CrossCutting.Outbox.Abstractions;
using CleanArch.Domain.Abstractions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanArch.CrossCutting.Outbox.Processing;

/// <summary>
/// Background service that processes outbox messages for reliable event publishing.
/// Ensures domain events are eventually published even if the initial publish fails.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);
    private const int BatchSize = 20;
    private const int MaxRetries = 3;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("📬 Outbox Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error processing outbox messages");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("📭 Outbox Processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        _logger.LogInformation("📨 Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var domainEventType = Type.GetType(message.Type);
                if (domainEventType == null)
                {
                    _logger.LogWarning("⚠️ Unknown event type: {Type}", message.Type);
                    message.Error = $"Unknown event type: {message.Type}";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(
                    message.Content,
                    domainEventType,
                    JsonOptions);

                if (domainEvent is IDomainEvent typedEvent)
                {
                    await publisher.Publish(typedEvent, cancellationToken);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    
                    _logger.LogDebug("✅ Processed outbox message {MessageId} of type {Type}",
                        message.Id, message.Type);
                }
                else
                {
                    message.Error = "Failed to deserialize domain event";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.ToString();
                
                _logger.LogError(ex,
                    "❌ Failed to process outbox message {MessageId} (Retry {RetryCount}/{MaxRetries})",
                    message.Id, message.RetryCount, MaxRetries);

                if (message.RetryCount >= MaxRetries)
                {
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    _logger.LogError(
                        "💀 Outbox message {MessageId} exceeded max retries and will be marked as processed",
                        message.Id);
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
