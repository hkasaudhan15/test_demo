using CleanArch.Application.Features.TenantSettings.Commands.BulkUpsert;
using CleanArch.Application.Features.TenantSettings.Commands.Delete;
using CleanArch.Application.Features.TenantSettings.Commands.ResetGroup;
using CleanArch.Application.Features.TenantSettings.Commands.Upsert;
using CleanArch.Application.Features.TenantSettings.Queries.GetByGroup;
using CleanArch.Application.Features.TenantSettings.Queries.GetEffective;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Presentation.Api.Controllers;

/// <summary>
/// Tenant Settings API — per-tenant configuration overrides.
///
/// Settings cascade: TenantSetting (override) → GlobalSetting (default).
/// Requires X-Tenant-Id header for tenant resolution.
///
/// Endpoints:
///   GET    /api/v1/tenant-settings                — all effective settings (merged)
///   GET    /api/v1/tenant-settings/group/{name}   — effective settings by group
///   PUT    /api/v1/tenant-settings                — upsert a single override
///   PUT    /api/v1/tenant-settings/bulk           — bulk upsert overrides
///   DELETE /api/v1/tenant-settings/{key}           — remove override (revert to global)
///   DELETE /api/v1/tenant-settings/group/{name}   — reset group to global defaults
/// </summary>
[Route("api/v{version:apiVersion}/tenant-settings")]
public sealed class TenantSettingsController : BaseApiController
{
    /// <summary>
    /// Gets all effective settings for the current tenant.
    /// Each setting indicates its source: Global (default) or TenantOverride.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllEffective()
    {
        var result = await Mediator.Send(new GetEffectiveSettingsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Gets effective settings for a specific group.
    /// </summary>
    [HttpGet("group/{group}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEffectiveByGroup(string group)
    {
        var result = await Mediator.Send(new GetEffectiveSettingsByGroupQuery(group));
        return HandleResult(result);
    }

    /// <summary>
    /// Creates or updates a tenant-specific setting override.
    /// The key must match an existing global setting (cannot create tenant-only keys).
    /// Read-only global settings cannot be overridden.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upsert([FromBody] UpsertTenantSettingCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Bulk create or update multiple tenant setting overrides.
    /// All settings validated against global settings before applying.
    /// </summary>
    [HttpPut("bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkUpsert([FromBody] BulkUpsertTenantSettingsCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Removes a tenant-specific override, reverting to the global default.
    /// </summary>
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string key)
    {
        var result = await Mediator.Send(new DeleteTenantSettingCommand(key));
        return HandleResult(result);
    }

    /// <summary>
    /// Resets all tenant overrides for a group back to global defaults.
    /// Returns the count of removed overrides.
    /// </summary>
    [HttpDelete("group/{group}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetGroup(string group)
    {
        var result = await Mediator.Send(new ResetTenantGroupCommand(group));
        return HandleResult(result);
    }
}
