using CleanArch.Domain.Abstractions.Entities;
using CleanArch.Domain.Abstractions.ValueObjects;
using CleanArch.Domain.Products.Events;

namespace CleanArch.Domain.Products;

/// <summary>
/// Product aggregate root — demonstrates the complete domain pattern:
///   - Private setters with factory method (Create)
///   - Domain events raised on state changes
///   - Value objects for complex properties (Money)
///   - Guard clauses for invariant protection
///
/// Flow:
///   Product.Create() → RaiseDomainEvent(ProductCreatedDomainEvent)
///     → SaveChanges → OutboxInterceptor serializes event to OutboxMessage
///     → OutboxProcessor deserializes → MediatR publishes
///     → ProductCreatedDomainEventHandler executes
/// </summary>
public sealed class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { } // EF Core

    private Product(Guid id, string name, string sku, string? description, Money price, int stockQuantity)
        : base(id)
    {
        Name = name;
        Sku = sku;
        Description = description;
        Price = price.Amount;
        Currency = price.Currency;
        StockQuantity = stockQuantity;
        IsActive = true;
    }

    public static Product Create(string name, string sku, string? description, Money price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));

        if (price.Amount < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

        var product = new Product(Guid.NewGuid(), name.Trim(), sku.Trim().ToUpperInvariant(), description?.Trim(), price, stockQuantity);

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id, product.Name, product.Price));

        return product;
    }

    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));

        if (newPrice.Currency != Currency)
            throw new InvalidOperationException($"Cannot change currency from {Currency} to {newPrice.Currency}.");

        var oldPrice = Price;
        Price = newPrice.Amount;

        RaiseDomainEvent(new ProductPriceChangedDomainEvent(Id, oldPrice, newPrice.Amount));
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void AdjustStock(int quantityChange)
    {
        var newQuantity = StockQuantity + quantityChange;
        if (newQuantity < 0)
            throw new InvalidOperationException($"Insufficient stock. Current: {StockQuantity}, requested change: {quantityChange}.");

        StockQuantity = newQuantity;
    }
}
