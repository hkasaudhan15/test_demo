using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.TenantSettings.Commands.ResetGroup;

/// <summary>
/// Removes all tenant overrides for a group, reverting to global defaults.
/// Returns the count of removed overrides.
/// </summary>
public sealed class ResetTenantGroupCommandHandler : ICommandHandler<ResetTenantGroupCommand, int>
{
    private readonly ITenantSettingsRepository _tenantSettingsRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantSettingsService _tenantSettingsService;

    public ResetTenantGroupCommandHandler(
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

    public async Task<Result<int>> Handle(ResetTenantGroupCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.TenantId))
            return Result.Failure<int>(TenantSettingErrors.TenantRequired);

        var tenantId = _currentUser.TenantId!;

        var overrides = await _tenantSettingsRepo.GetByGroupAsync(tenantId, request.Group, cancellationToken);
        if (overrides.Count == 0)
            return Result.Success(0);

        _tenantSettingsRepo.RemoveRange(overrides);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var setting in overrides)
            await _tenantSettingsService.InvalidateTenantCacheAsync(tenantId, setting.Key, cancellationToken);

        await _tenantSettingsService.InvalidateTenantGroupCacheAsync(tenantId, request.Group, cancellationToken);

        return Result.Success(overrides.Count);
    }
}
