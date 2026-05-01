using CleanArch.Application.Abstractions.Messaging.Queries;

namespace CleanArch.Application.Features.Products.Queries.GetById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;
