using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.Products.Commands.Create;

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity) : ICommand<Guid>;
