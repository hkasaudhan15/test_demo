using CleanArch.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

public sealed class TenantFeatureConfiguration : IEntityTypeConfiguration<TenantFeature>
{
    public void Configure(EntityTypeBuilder<TenantFeature> builder)
    {
        builder.ToTable("TenantFeatures");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FeatureCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(f => new { f.TenantId, f.FeatureCode })
            .IsUnique()
            .HasDatabaseName("IX_TenantFeatures_TenantId_FeatureCode");
    }
}
