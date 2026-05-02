using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants.Commands.Register;

public sealed class RegisterTenantCommandHandler : ICommandHandler<RegisterTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterTenantCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        if (await _tenantRepository.ExistsAsync(request.Identifier, cancellationToken))
            return Result.Failure<Guid>(TenantErrors.DuplicateIdentifier(request.Identifier));

        var tenant = Tenant.Register(
            request.Identifier,
            request.Name,
            request.ContactEmail,
            request.AdminName,
            request.Description,
            request.Tier);

        if (request.AutoActivate)
            tenant.Activate();

        _tenantRepository.Add(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tenant.Id);
    }
}
