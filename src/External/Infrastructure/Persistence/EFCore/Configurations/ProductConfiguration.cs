using CleanArch.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

/// <summary>
/// EF Core configuration for the Product aggregate.
/// Only declares table-specific columns and indexes — audit, soft-delete,
/// and concurrency are handled by ApplyDomainConventions.
/// </summary>
public sealed class ProductConfiguration : AggregateRootConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.StockQuantity)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique SKU constraint
        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("IX_Products_Sku");

        // Active products query index
        builder.HasIndex(p => new { p.IsActive, p.Name })
            .HasDatabaseName("IX_Products_IsActive_Name");
    }
}
