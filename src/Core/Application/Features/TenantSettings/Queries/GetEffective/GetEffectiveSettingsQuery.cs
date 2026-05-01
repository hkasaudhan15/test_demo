using CleanArch.Application.Abstractions.Messaging.Queries;

namespace CleanArch.Application.Features.TenantSettings.Queries.GetEffective;

/// <summary>
/// Gets all effective settings for the current tenant — global defaults merged
/// with tenant-specific overrides. Each setting indicates its source.
/// </summary>
public sealed record GetEffectiveSettingsQuery : IQuery<IReadOnlyList<EffectiveSettingResponse>>;
