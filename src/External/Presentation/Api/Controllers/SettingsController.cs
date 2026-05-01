using CleanArch.Application.Features.Settings.Commands.Delete;
using CleanArch.Application.Features.Settings.Commands.Upsert;
using CleanArch.Application.Features.Settings.Queries.GetAll;
using CleanArch.Application.Features.Settings.Queries.GetByGroup;
using CleanArch.Application.Features.Settings.Queries.GetByKey;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Presentation.Api.Controllers;

/// <summary>
/// Global Settings API — manage application-wide configuration stored in database.
///
/// Settings are grouped by category (General, Email, Security, UI, etc.),
/// cached with cache-aside pattern, and audited (who changed what, when).
///
/// Endpoints:
///   GET  /api/v1/settings           — all settings (grouped, ordered)
///   GET  /api/v1/settings/{key}     — single setting by key
///   GET  /api/v1/settings/group/{g} — all settings in a group
///   PUT  /api/v1/settings           — create or update a setting
///   DELETE /api/v1/settings/{key}   — delete a setting
/// </summary>
public sealed class SettingsController : BaseApiController
{
    /// <summary>
    /// Gets all settings, ordered by group → display order → key.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllSettingsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Gets a single setting by its unique key.
    /// </summary>
    [HttpGet("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByKey(string key)
    {
        var result = await Mediator.Send(new GetSettingByKeyQuery(key));
        return HandleResult(result);
    }

    /// <summary>
    /// Gets all settings belonging to a group (e.g., "Security", "Email").
    /// </summary>
    [HttpGet("group/{group}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGroup(string group)
    {
        var result = await Mediator.Send(new GetSettingsByGroupQuery(group));
        return HandleResult(result);
    }

    /// <summary>
    /// Creates or updates a setting. Upsert semantics: if the key exists,
    /// updates the value; if not, creates a new setting.
    /// Read-only settings cannot be updated (returns 403).
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upsert([FromBody] UpsertSettingCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Deletes a setting by key. Read-only settings cannot be deleted (returns 403).
    /// </summary>
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(string key)
    {
        var result = await Mediator.Send(new DeleteSettingCommand(key));
        return HandleResult(result);
    }
}
