using CleanArch.CrossCutting.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="IdempotentRequest"/> table.
/// The PK on <c>Id</c> (which is the idempotency key) serves as both
/// the lookup index and the unique constraint that prevents duplicates.
/// </summary>
public sealed class IdempotentRequestConfiguration : IEntityTypeConfiguration<IdempotentRequest>
{
    public void Configure(EntityTypeBuilder<IdempotentRequest> builder)
    {
        builder.ToTable("IdempotentRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.CommandName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.CreatedOnUtc)
            .IsRequired();

        // Index for cleanup queries (e.g. delete requests older than N days)
        builder.HasIndex(r => r.CreatedOnUtc)
            .HasDatabaseName("IX_IdempotentRequests_CreatedOnUtc");
    }
}
