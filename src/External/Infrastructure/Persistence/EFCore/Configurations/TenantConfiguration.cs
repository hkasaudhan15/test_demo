using CleanArch.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Identifier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.ContactEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.AdminName)
            .HasMaxLength(256);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Tier)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.ConnectionString)
            .HasMaxLength(1000);

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(2000);

        builder.Property(t => t.CustomDomain)
            .HasMaxLength(256);

        builder.Property(t => t.SuspensionReason)
            .HasMaxLength(500);

        // ─── Indexes ─────────────────────────────────────
        builder.HasIndex(t => t.Identifier)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_Identifier");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_Tenants_Status");

        builder.HasIndex(t => t.CustomDomain)
            .IsUnique()
            .HasFilter("[CustomDomain] IS NOT NULL")
            .HasDatabaseName("IX_Tenants_CustomDomain");

        // ─── Feature Flags (owned collection) ────────────
        builder.HasMany(t => t.Features)
            .WithOne()
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Ignore domain events (handled by convention) ─
        builder.Ignore(t => t.DomainEvents);

        // ─── Seed ─────────────────────────────────────────
        SeedDefaults(builder);
    }

    private static void SeedDefaults(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasData(
            new
            {
                Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"),
                Identifier = "default",
                Name = "Default Tenant",
                Description = "System default tenant for development and testing.",
                ContactEmail = "admin@example.com",
                AdminName = "System Administrator",
                Status = TenantStatus.Active,
                Tier = SubscriptionTier.Enterprise,
                ConnectionString = (string?)null,
                LogoUrl = (string?)null,
                CustomDomain = (string?)null,
                MaxUsers = int.MaxValue,
                MaxStorageMb = int.MaxValue,
                MaxApiCallsPerDay = int.MaxValue,
                MaxProjectsOrWorkspaces = int.MaxValue,
                ActivatedOnUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SuspendedOnUtc = (DateTime?)null,
                SuspensionReason = (string?)null,
                SubscriptionExpiresOnUtc = (DateTime?)null,
                CreatedOnUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "System",
                ModifiedOnUtc = (DateTime?)null,
                ModifiedBy = (string?)null,
                IsDeleted = false,
                DeletedOnUtc = (DateTime?)null,
                DeletedBy = (string?)null,
                RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }
            },
            new
            {
                Id = Guid.Parse("a0000001-0000-0000-0000-000000000002"),
                Identifier = "demo",
                Name = "Demo Tenant",
                Description = "Demonstration tenant for showcasing features.",
                ContactEmail = "demo@example.com",
                AdminName = "Demo Admin",
                Status = TenantStatus.Active,
                Tier = SubscriptionTier.Professional,
                ConnectionString = (string?)null,
                LogoUrl = (string?)null,
                CustomDomain = (string?)null,
                MaxUsers = 100,
                MaxStorageMb = 50000,
                MaxApiCallsPerDay = 100000,
                MaxProjectsOrWorkspaces = 100,
                ActivatedOnUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SuspendedOnUtc = (DateTime?)null,
                SuspensionReason = (string?)null,
                SubscriptionExpiresOnUtc = (DateTime?)null,
                CreatedOnUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "System",
                ModifiedOnUtc = (DateTime?)null,
                ModifiedBy = (string?)null,
                IsDeleted = false,
                DeletedOnUtc = (DateTime?)null,
                DeletedBy = (string?)null,
                RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }
            }
        );
    }
}
