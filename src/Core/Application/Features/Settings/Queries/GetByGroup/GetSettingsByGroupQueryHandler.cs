using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Application.Features.Settings.Queries.GetByKey;
using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Application.Features.Settings.Queries.GetByGroup;

public sealed class GetSettingsByGroupQueryHandler
    : IQueryHandler<GetSettingsByGroupQuery, IReadOnlyList<SettingResponse>>
{
    private readonly ISettingsRepository _settingsRepository;

    public GetSettingsByGroupQueryHandler(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<IReadOnlyList<SettingResponse>>> Handle(
        GetSettingsByGroupQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetByGroupAsync(request.Group, cancellationToken);

        var responses = settings
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Key)
            .Select(GetSettingByKeyQueryHandler.MapToResponse)
            .ToList();

        return Result.Success<IReadOnlyList<SettingResponse>>(responses);
    }
}
