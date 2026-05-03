using CleanArch.Application.Abstractions.Messaging.Queries;

namespace CleanArch.Application.Features.TenantSettings.Queries.GetByGroup;

/// <summary>
/// Gets effective settings for a specific group for the current tenant.
/// Returns global defaults merged with tenant overrides, indicating source.
/// </summary>
public sealed record GetEffectiveSettingsByGroupQuery(string Group)
    : IQuery<IReadOnlyList<Queries.EffectiveSettingResponse>>;
