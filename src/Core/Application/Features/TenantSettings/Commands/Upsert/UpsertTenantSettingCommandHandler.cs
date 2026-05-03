using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.TenantSettings.Commands.Upsert;

/// <summary>
/// Upserts a tenant setting override. Validates that a matching global setting exists
/// (tenant settings can only override existing global keys). Inherits group and data type
/// from the global setting to ensure consistency.
/// </summary>
public sealed class UpsertTenantSettingCommandHandler : ICommandHandler<UpsertTenantSettingCommand, Guid>
{
    private readonly ITenantSettingsRepository _tenantSettingsRepo;
    private readonly ISettingsRepository _globalSettingsRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantSettingsService _tenantSettingsService;

    public UpsertTenantSettingCommandHandler(
        ITenantSettingsRepository tenantSettingsRepo,
        ISettingsRepository globalSettingsRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ITenantSettingsService tenantSettingsService)
    {
        _tenantSettingsRepo = tenantSettingsRepo;
        _globalSettingsRepo = globalSettingsRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tenantSettingsService = tenantSettingsService;
    }

    public async Task<Result<Guid>> Handle(UpsertTenantSettingCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.TenantId))
            return Result.Failure<Guid>(TenantSettingErrors.TenantRequired);

        var tenantId = _currentUser.TenantId!;

        var globalSetting = await _globalSettingsRepo.GetByKeyAsync(request.Key, cancellationToken);
        if (globalSetting is null)
            return Result.Failure<Guid>(TenantSettingErrors.GlobalSettingNotFound(request.Key));

        if (globalSetting.IsReadOnly)
            return Result.Failure<Guid>(SettingErrors.ReadOnly);

        var existing = await _tenantSettingsRepo.GetByKeyAsync(tenantId, request.Key, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateValue(request.Value);
            existing.UpdateDescription(request.Description);
            _tenantSettingsRepo.Update(existing);
        }
        else
        {
            existing = TenantSetting.Create(
                tenantId,
                request.Key,
                request.Value,
                globalSetting.Group,
                globalSetting.DataType,
                request.Description);

            _tenantSettingsRepo.Add(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _tenantSettingsService.InvalidateTenantCacheAsync(tenantId, request.Key, cancellationToken);
        await _tenantSettingsService.InvalidateTenantGroupCacheAsync(tenantId, globalSetting.Group, cancellationToken);

        return Result.Success(existing.Id);
    }
}
