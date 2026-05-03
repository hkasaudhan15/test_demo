using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.TenantSettings.Commands.Delete;

/// <summary>
/// Removes a tenant-specific setting override, reverting to the global default.
/// </summary>
public sealed record DeleteTenantSettingCommand(string Key) : ICommand;
