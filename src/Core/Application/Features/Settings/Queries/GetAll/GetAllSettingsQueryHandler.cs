using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Application.Features.Settings.Queries.GetByKey;
using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Application.Features.Settings.Queries.GetAll;

public sealed class GetAllSettingsQueryHandler
    : IQueryHandler<GetAllSettingsQuery, IReadOnlyList<SettingResponse>>
{
    private readonly ISettingsRepository _settingsRepository;

    public GetAllSettingsQueryHandler(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<IReadOnlyList<SettingResponse>>> Handle(
        GetAllSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetAllAsync(cancellationToken);

        var responses = settings
            .OrderBy(s => s.Group)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Key)
            .Select(GetSettingByKeyQueryHandler.MapToResponse)
            .ToList();

        return Result.Success<IReadOnlyList<SettingResponse>>(responses);
    }
}
