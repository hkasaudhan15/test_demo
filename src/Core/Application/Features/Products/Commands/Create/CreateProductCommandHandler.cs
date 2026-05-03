using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Abstractions.ValueObjects;
using CleanArch.Domain.Products;
using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Application.Features.Products.Commands.Create;

/// <summary>
/// Creates a new product and persists it.
///
/// Domain event flow:
///   1. Product.Create() raises ProductCreatedDomainEvent
///   2. unitOfWork.SaveChangesAsync() triggers OutboxInterceptor
///   3. OutboxInterceptor serializes the event into OutboxMessage table
///   4. OutboxProcessor (background) picks up the message
///   5. OutboxProcessor deserializes and publishes via MediatR
///   6. ProductCreatedDomainEventHandler executes asynchronously
/// </summary>
public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IRepository<Product> productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var normalizedSku = request.Sku.Trim().ToUpperInvariant();
        var existsWithSku = await _productRepository.ExistsAsync(
            p => p.Sku == normalizedSku,
            cancellationToken);

        if (existsWithSku)
            return Result.Failure<Guid>(ProductErrors.DuplicateSku(request.Sku));

        var price = Money.Create(request.Price, request.Currency);

        var product = Product.Create(
            request.Name,
            request.Sku,
            request.Description,
            price,
            request.StockQuantity);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
