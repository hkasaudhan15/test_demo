using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.Settings.Commands.Delete;

public sealed record DeleteSettingCommand(string Key) : ICommand;
