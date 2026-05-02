using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Queries.GetAll;

public sealed record GetAllTenantsQuery(TenantStatus? StatusFilter = null) : IQuery<IReadOnlyList<TenantResponse>>;
