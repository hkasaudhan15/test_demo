using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.Settings.Queries.GetByKey;

public sealed class GetSettingByKeyQueryHandler : IQueryHandler<GetSettingByKeyQuery, SettingResponse>
{
    private readonly ISettingsRepository _settingsRepository;

    public GetSettingByKeyQueryHandler(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<SettingResponse>> Handle(GetSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        var setting = await _settingsRepository.GetByKeyAsync(request.Key, cancellationToken);

        if (setting is null)
            return Result.Failure<SettingResponse>(SettingErrors.NotFound(request.Key));

        return Result.Success(MapToResponse(setting));
    }

    internal static SettingResponse MapToResponse(GlobalSetting setting) => new(
        setting.Id,
        setting.Key,
        setting.Value,
        setting.Group,
        setting.DataType,
        setting.Description,
        setting.IsReadOnly,
        setting.DisplayOrder,
        setting.CreatedOnUtc,
        setting.ModifiedOnUtc,
        setting.ModifiedBy);
}
