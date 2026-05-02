using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Commands.UpdateProfile;

public sealed class UpdateTenantProfileCommandHandler : ICommandHandler<UpdateTenantProfileCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTenantProfileCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTenantProfileCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure(TenantErrors.NotFound(request.TenantId));

        tenant.UpdateProfile(request.Name, request.Description, request.ContactEmail, request.AdminName);
        tenant.SetLogoUrl(request.LogoUrl);
        tenant.SetCustomDomain(request.CustomDomain);

        _tenantRepository.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
