using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.Settings.Commands.Upsert;

public sealed record UpsertSettingCommand(
    string Key,
    string Value,
    string Group,
    SettingDataType DataType = SettingDataType.Text,
    string? Description = null,
    bool IsReadOnly = false,
    int DisplayOrder = 0) : ICommand<Guid>;
