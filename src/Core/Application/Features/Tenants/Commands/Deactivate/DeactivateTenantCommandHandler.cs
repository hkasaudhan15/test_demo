using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Commands.Deactivate;

public sealed class DeactivateTenantCommandHandler : ICommandHandler<DeactivateTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateTenantCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure(TenantErrors.NotFound(request.TenantId));

        tenant.Deactivate(request.Reason);
        _tenantRepository.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
