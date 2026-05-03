using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Application.Features.Tenants.Queries.GetAll;

public sealed class GetAllTenantsQueryHandler : IQueryHandler<GetAllTenantsQuery, IReadOnlyList<TenantResponse>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetAllTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<IReadOnlyList<TenantResponse>>> Handle(
        GetAllTenantsQuery request,
        CancellationToken cancellationToken)
    {
        var tenants = request.StatusFilter.HasValue
            ? await _tenantRepository.GetByStatusAsync(request.StatusFilter.Value, cancellationToken)
            : await _tenantRepository.GetAllAsync(cancellationToken);

        var responses = tenants
            .Select(TenantMapper.MapToResponse)
            .ToList();

        return Result.Success<IReadOnlyList<TenantResponse>>(responses);
    }
}
