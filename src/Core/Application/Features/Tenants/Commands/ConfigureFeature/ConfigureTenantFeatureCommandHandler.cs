using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Commands.ConfigureFeature;

public sealed class ConfigureTenantFeatureCommandHandler : ICommandHandler<ConfigureTenantFeatureCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfigureTenantFeatureCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ConfigureTenantFeatureCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure(TenantErrors.NotFound(request.TenantId));

        if (request.IsEnabled)
            tenant.EnableFeature(request.FeatureCode, request.ExpiresOnUtc);
        else
            tenant.DisableFeature(request.FeatureCode);

        _tenantRepository.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
