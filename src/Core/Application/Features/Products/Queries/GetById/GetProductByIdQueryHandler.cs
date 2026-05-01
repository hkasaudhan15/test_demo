using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Products;
using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Application.Features.Products.Queries.GetById;

public sealed class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    private readonly IRepository<Product> _productRepository;

    public GetProductByIdQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.Id));

        var response = new ProductResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Description,
            product.Price,
            product.Currency,
            product.StockQuantity,
            product.IsActive,
            product.CreatedOnUtc);

        return Result.Success(response);
    }
}
