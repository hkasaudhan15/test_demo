using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.TenantSettings.Commands.Delete;

public sealed class DeleteTenantSettingCommandHandler : ICommandHandler<DeleteTenantSettingCommand>
{
    private readonly ITenantSettingsRepository _tenantSettingsRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantSettingsService _tenantSettingsService;

    public DeleteTenantSettingCommandHandler(
        ITenantSettingsRepository tenantSettingsRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ITenantSettingsService tenantSettingsService)
    {
        _tenantSettingsRepo = tenantSettingsRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tenantSettingsService = tenantSettingsService;
    }

    public async Task<Result> Handle(DeleteTenantSettingCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.TenantId))
            return Result.Failure(TenantSettingErrors.TenantRequired);

        var tenantId = _currentUser.TenantId!;

        var setting = await _tenantSettingsRepo.GetByKeyAsync(tenantId, request.Key, cancellationToken);
        if (setting is null)
            return Result.Failure(TenantSettingErrors.NotFound(tenantId, request.Key));

        var group = setting.Group;

        _tenantSettingsRepo.Remove(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _tenantSettingsService.InvalidateTenantCacheAsync(tenantId, request.Key, cancellationToken);
        await _tenantSettingsService.InvalidateTenantGroupCacheAsync(tenantId, group, cancellationToken);

        return Result.Success();
    }
}
