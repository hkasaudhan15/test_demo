using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.TenantSettings.Commands.ResetGroup;

/// <summary>
/// Removes all tenant-specific overrides for a group, reverting to global defaults.
/// </summary>
public sealed record ResetTenantGroupCommand(string Group) : ICommand<int>;
