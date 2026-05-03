using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Queries.GetById;

public sealed class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantResponse>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantByIdQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantResponse>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tenant is null)
            return Result.Failure<TenantResponse>(TenantErrors.NotFound(request.Id));

        return Result.Success(TenantMapper.MapToResponse(tenant));
    }
}
