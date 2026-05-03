using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.TenantSettings.Commands.BulkUpsert;

/// <summary>
/// Bulk create or update multiple tenant settings in a single transaction.
/// Useful for tenant onboarding or settings import.
/// </summary>
public sealed record BulkUpsertTenantSettingsCommand(
    IReadOnlyList<TenantSettingItem> Settings) : ICommand<int>;

public sealed record TenantSettingItem(
    string Key,
    string Value,
    string? Description = null);
