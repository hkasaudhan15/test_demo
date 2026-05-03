using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Domain.Products;

/// <summary>
/// Static error catalog for the Product bounded context.
/// Each error maps to a specific HTTP status code via ErrorType.
/// </summary>
public static class ProductErrors
{
    public static Error NotFound(Guid productId) =>
        new("Product.NotFound", $"Product with ID '{productId}' was not found.", ErrorType.NotFound);

    public static Error DuplicateSku(string sku) =>
        new("Product.DuplicateSku", $"A product with SKU '{sku}' already exists.", ErrorType.Conflict);
}
