using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.TenantSettings.Queries.GetEffective;

public sealed class GetEffectiveSettingsQueryHandler
    : IQueryHandler<GetEffectiveSettingsQuery, IReadOnlyList<EffectiveSettingResponse>>
{
    private readonly ITenantSettingsService _tenantSettingsService;
    private readonly ICurrentUserService _currentUser;

    public GetEffectiveSettingsQueryHandler(
        ITenantSettingsService tenantSettingsService,
        ICurrentUserService currentUser)
    {
        _tenantSettingsService = tenantSettingsService;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<EffectiveSettingResponse>>> Handle(
        GetEffectiveSettingsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.TenantId))
            return Result.Failure<IReadOnlyList<EffectiveSettingResponse>>(TenantSettingErrors.TenantRequired);

        var effective = await _tenantSettingsService.GetAllEffectiveAsync(
            _currentUser.TenantId!, cancellationToken);

        var responses = effective.Values
            .OrderBy(s => s.Group)
            .ThenBy(s => s.Key)
            .Select(s => new EffectiveSettingResponse(
                s.Key, s.Value, s.Group,
                SettingDataType.Text, // DataType not tracked in EffectiveSetting
                s.Source, s.Description,
                null, null))
            .ToList();

        return Result.Success<IReadOnlyList<EffectiveSettingResponse>>(responses);
    }
}
