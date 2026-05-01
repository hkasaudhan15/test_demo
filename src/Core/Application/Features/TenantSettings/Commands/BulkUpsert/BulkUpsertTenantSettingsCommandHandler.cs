using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.TenantSettings.Commands.BulkUpsert;

/// <summary>
/// Bulk upserts tenant settings. Validates each key against global settings.
/// Performs all changes in a single transaction. Returns the count of upserted settings.
/// </summary>
public sealed class BulkUpsertTenantSettingsCommandHandler
    : ICommandHandler<BulkUpsertTenantSettingsCommand, int>
{
    private readonly ITenantSettingsRepository _tenantSettingsRepo;
    private readonly ISettingsRepository _globalSettingsRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantSettingsService _tenantSettingsService;

    public BulkUpsertTenantSettingsCommandHandler(
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

    public async Task<Result<int>> Handle(
        BulkUpsertTenantSettingsCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.TenantId))
            return Result.Failure<int>(TenantSettingErrors.TenantRequired);

        var tenantId = _currentUser.TenantId!;
        var affectedGroups = new HashSet<string>();
        var count = 0;

        foreach (var item in request.Settings)
        {
            var globalSetting = await _globalSettingsRepo.GetByKeyAsync(item.Key, cancellationToken);
            if (globalSetting is null)
                return Result.Failure<int>(TenantSettingErrors.GlobalSettingNotFound(item.Key));

            if (globalSetting.IsReadOnly)
                continue;

            var existing = await _tenantSettingsRepo.GetByKeyAsync(tenantId, item.Key, cancellationToken);

            if (existing is not null)
            {
                existing.UpdateValue(item.Value);
                existing.UpdateDescription(item.Description);
                _tenantSettingsRepo.Update(existing);
            }
            else
            {
                var tenantSetting = TenantSetting.Create(
                    tenantId,
                    item.Key,
                    item.Value,
                    globalSetting.Group,
                    globalSetting.DataType,
                    item.Description);

                _tenantSettingsRepo.Add(tenantSetting);
            }

            affectedGroups.Add(globalSetting.Group);
            count++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var item in request.Settings)
            await _tenantSettingsService.InvalidateTenantCacheAsync(tenantId, item.Key, cancellationToken);

        foreach (var group in affectedGroups)
            await _tenantSettingsService.InvalidateTenantGroupCacheAsync(tenantId, group, cancellationToken);

        return Result.Success(count);
    }
}
