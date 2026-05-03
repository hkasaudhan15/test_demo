using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.Settings.Commands.Delete;

public sealed class DeleteSettingCommandHandler : ICommandHandler<DeleteSettingCommand>
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingsService _settingsService;

    public DeleteSettingCommandHandler(
        ISettingsRepository settingsRepository,
        IUnitOfWork unitOfWork,
        ISettingsService settingsService)
    {
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _settingsService = settingsService;
    }

    public async Task<Result> Handle(DeleteSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _settingsRepository.GetByKeyAsync(request.Key, cancellationToken);

        if (setting is null)
            return Result.Failure(SettingErrors.NotFound(request.Key));

        if (setting.IsReadOnly)
            return Result.Failure(SettingErrors.ReadOnly);

        _settingsRepository.Remove(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _settingsService.InvalidateCacheAsync(request.Key, cancellationToken);
        await _settingsService.InvalidateGroupCacheAsync(setting.Group, cancellationToken);

        return Result.Success();
    }
}
