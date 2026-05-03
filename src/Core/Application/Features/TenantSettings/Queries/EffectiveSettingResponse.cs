using CleanArch.Application.Abstractions.Settings;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.TenantSettings.Queries;

public sealed record EffectiveSettingResponse(
    string Key,
    string Value,
    string Group,
    SettingDataType DataType,
    SettingSource Source,
    string? Description,
    DateTime? ModifiedOnUtc,
    string? ModifiedBy);
