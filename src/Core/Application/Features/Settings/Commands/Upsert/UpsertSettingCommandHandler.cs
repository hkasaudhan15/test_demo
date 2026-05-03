using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.Settings.Commands.Upsert;

/// <summary>
/// Upsert (create or update) a setting. If the key exists, updates the value;
/// if not, creates a new setting. Invalidates the cache after mutation.
/// </summary>
public sealed class UpsertSettingCommandHandler : ICommandHandler<UpsertSettingCommand, Guid>
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingsService _settingsService;

    public UpsertSettingCommandHandler(
        ISettingsRepository settingsRepository,
        IUnitOfWork unitOfWork,
        ISettingsService settingsService)
    {
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _settingsService = settingsService;
    }

    public async Task<Result<Guid>> Handle(UpsertSettingCommand request, CancellationToken cancellationToken)
    {
        var existing = await _settingsRepository.GetByKeyAsync(request.Key, cancellationToken);

        if (existing is not null)
        {
            if (existing.IsReadOnly)
                return Result.Failure<Guid>(SettingErrors.ReadOnly);

            existing.UpdateValue(request.Value);
            existing.UpdateMetadata(request.Description, request.DisplayOrder);
            _settingsRepository.Update(existing);
        }
        else
        {
            existing = GlobalSetting.Create(
                request.Key,
                request.Value,
                request.Group,
                request.DataType,
                request.Description,
                request.IsReadOnly,
                request.DisplayOrder);

            _settingsRepository.Add(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _settingsService.InvalidateCacheAsync(request.Key, cancellationToken);
        await _settingsService.InvalidateGroupCacheAsync(request.Group, cancellationToken);

        return Result.Success(existing.Id);
    }
}
