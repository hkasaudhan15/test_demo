using CleanArch.CrossCutting.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="OutboxMessage"/> table.
/// Indexes are designed for the OutboxProcessor polling query:
///   WHERE Status = Pending AND (NextRetryOnUtc IS NULL OR NextRetryOnUtc &lt;= @now)
///   ORDER BY OccurredOnUtc
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.EventType)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(m => m.Payload)
            .IsRequired();

        builder.Property(m => m.OccurredOnUtc)
            .IsRequired();

        builder.Property(m => m.Error)
            .HasMaxLength(4000);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();

        // Composite index for the processor polling query.
        // Covers: Status = Pending, NextRetryOnUtc filter, OccurredOnUtc ordering.
        builder.HasIndex(m => new { m.Status, m.NextRetryOnUtc, m.OccurredOnUtc })
            .HasDatabaseName("IX_OutboxMessages_Processor");

        // Index for querying processed/failed messages (monitoring, cleanup jobs).
        builder.HasIndex(m => m.ProcessedOnUtc)
            .HasDatabaseName("IX_OutboxMessages_ProcessedOnUtc");
    }
}
