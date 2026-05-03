using CleanArch.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

public sealed class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> builder)
    {
        builder.ToTable("TenantSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(s => s.Group)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.DataType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        // Unique constraint: one override per tenant per key
        builder.HasIndex(s => new { s.TenantId, s.Key })
            .IsUnique()
            .HasDatabaseName("IX_TenantSettings_TenantId_Key");

        // Tenant + group lookup
        builder.HasIndex(s => new { s.TenantId, s.Group })
            .HasDatabaseName("IX_TenantSettings_TenantId_Group");

        // All overrides for a tenant
        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("IX_TenantSettings_TenantId");
    }
}
