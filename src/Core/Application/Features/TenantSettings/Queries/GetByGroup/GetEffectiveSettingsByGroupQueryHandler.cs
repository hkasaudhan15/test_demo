using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.TenantSettings.Queries.GetByGroup;

public sealed class GetEffectiveSettingsByGroupQueryHandler
    : IQueryHandler<GetEffectiveSettingsByGroupQuery, IReadOnlyList<Queries.EffectiveSettingResponse>>
{
    private readonly ITenantSettingsService _tenantSettingsService;
    private readonly ICurrentUserService _currentUser;

    public GetEffectiveSettingsByGroupQueryHandler(
        ITenantSettingsService tenantSettingsService,
        ICurrentUserService currentUser)
    {
        _tenantSettingsService = tenantSettingsService;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<Queries.EffectiveSettingResponse>>> Handle(
        GetEffectiveSettingsByGroupQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.TenantId))
            return Result.Failure<IReadOnlyList<Queries.EffectiveSettingResponse>>(TenantSettingErrors.TenantRequired);

        var effective = await _tenantSettingsService.GetEffectiveByGroupAsync(
            _currentUser.TenantId!, request.Group, cancellationToken);

        var responses = effective.Values
            .OrderBy(s => s.Key)
            .Select(s => new Queries.EffectiveSettingResponse(
                s.Key, s.Value, s.Group,
                SettingDataType.Text,
                s.Source, s.Description,
                null, null))
            .ToList();

        return Result.Success<IReadOnlyList<Queries.EffectiveSettingResponse>>(responses);
    }
}
