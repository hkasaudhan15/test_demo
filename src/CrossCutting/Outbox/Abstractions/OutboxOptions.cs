namespace CleanArch.CrossCutting.Outbox.Abstractions;

/// <summary>
/// Configuration for the outbox background processor.
/// Bind from <c>appsettings.json</c> section <c>"Outbox"</c>.
/// </summary>
public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>
    /// How often the processor polls for pending messages. Default: 10 seconds.
    /// </summary>
    public int IntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Maximum messages fetched per poll cycle. Default: 20.
    /// </summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// Maximum retry attempts before a message is dead-lettered. Default: 5.
    /// </summary>
    public int MaxRetries { get; set; } = 5;
}
