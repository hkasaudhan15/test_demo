using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.Settings.Queries.GetByKey;

public sealed record SettingResponse(
    Guid Id,
    string Key,
    string Value,
    string Group,
    SettingDataType DataType,
    string? Description,
    bool IsReadOnly,
    int DisplayOrder,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc,
    string? ModifiedBy);
