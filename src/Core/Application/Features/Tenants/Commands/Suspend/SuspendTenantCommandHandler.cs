using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Commands.Suspend;

public sealed class SuspendTenantCommandHandler : ICommandHandler<SuspendTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendTenantCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure(TenantErrors.NotFound(request.TenantId));

        tenant.Suspend(request.Reason);
        _tenantRepository.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
