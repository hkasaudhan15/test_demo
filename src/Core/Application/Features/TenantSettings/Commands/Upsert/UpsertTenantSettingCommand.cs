using CleanArch.Application.Abstractions.Messaging.Commands;
using CleanArch.Domain.Settings;

namespace CleanArch.Application.Features.TenantSettings.Commands.Upsert;

/// <summary>
/// Creates or updates a tenant-specific setting override.
/// The tenant is resolved from the current request context (X-Tenant-Id header).
/// </summary>
public sealed record UpsertTenantSettingCommand(
    string Key,
    string Value,
    string? Description = null) : ICommand<Guid>;
