namespace CleanArch.Application.Features.Products.Queries.GetById;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedOnUtc);
