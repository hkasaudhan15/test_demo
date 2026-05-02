using CleanArch.Application.Abstractions.Messaging.Queries;

namespace CleanArch.Application.Features.Tenants.Queries.GetById;

public sealed record GetTenantByIdQuery(Guid Id) : IQuery<TenantResponse>;
