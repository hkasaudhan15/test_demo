using CleanArch.Application.Features.Tenants.Commands.Activate;
using CleanArch.Application.Features.Tenants.Commands.ConfigureFeature;
using CleanArch.Application.Features.Tenants.Commands.Deactivate;
using CleanArch.Application.Features.Tenants.Commands.Register;
using CleanArch.Application.Features.Tenants.Commands.Suspend;
using CleanArch.Application.Features.Tenants.Commands.UpdateProfile;
using CleanArch.Application.Features.Tenants.Commands.UpdateSubscription;
using CleanArch.Application.Features.Tenants.Queries.GetAll;
using CleanArch.Application.Features.Tenants.Queries.GetById;
using CleanArch.Domain.Tenants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Presentation.Api.Controllers;

/// <summary>
/// Tenant Administration API — full tenant lifecycle management.
///
/// Endpoints:
///   POST   /api/v1/tenants                          — register new tenant
///   GET    /api/v1/tenants                          — list all tenants (optional status filter)
///   GET    /api/v1/tenants/{id}                     — get tenant by ID
///   PUT    /api/v1/tenants/{id}/profile              — update tenant profile
///   PUT    /api/v1/tenants/{id}/subscription          — change subscription tier
///   PUT    /api/v1/tenants/{id}/features              — configure feature flag
///   POST   /api/v1/tenants/{id}/activate              — activate tenant
///   POST   /api/v1/tenants/{id}/suspend               — suspend tenant
///   POST   /api/v1/tenants/{id}/deactivate            — deactivate tenant
/// </summary>
[Route("api/v{version:apiVersion}/tenants")]
public sealed class TenantsController : BaseApiController
{
    /// <summary>
    /// Registers a new tenant with optional auto-activation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterTenantCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreated(result, nameof(GetById), new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Lists all tenants, optionally filtered by status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TenantStatus? status = null)
    {
        var result = await Mediator.Send(new GetAllTenantsQuery(status));
        return HandleResult(result);
    }

    /// <summary>
    /// Gets a tenant by ID, including feature flags.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetTenantByIdQuery(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Updates tenant profile (name, email, logo, custom domain).
    /// </summary>
    [HttpPut("{id:guid}/profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateTenantProfileRequest request)
    {
        var command = new UpdateTenantProfileCommand(
            id, request.Name, request.ContactEmail, request.AdminName,
            request.Description, request.LogoUrl, request.CustomDomain);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Changes tenant subscription tier and resource limits.
    /// </summary>
    [HttpPut("{id:guid}/subscription")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] UpdateSubscriptionRequest request)
    {
        var command = new UpdateTenantSubscriptionCommand(id, request.Tier, request.ExpiresOnUtc);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Configures a feature flag for the tenant.
    /// </summary>
    [HttpPut("{id:guid}/features")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfigureFeature(Guid id, [FromBody] ConfigureFeatureRequest request)
    {
        var command = new ConfigureTenantFeatureCommand(id, request.FeatureCode, request.IsEnabled, request.ExpiresOnUtc);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Activates a tenant (sets status to Active).
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await Mediator.Send(new ActivateTenantCommand(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Suspends a tenant temporarily with a reason.
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendRequest request)
    {
        var result = await Mediator.Send(new SuspendTenantCommand(id, request.Reason));
        return HandleResult(result);
    }

    /// <summary>
    /// Deactivates a tenant with a reason.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateRequest request)
    {
        var result = await Mediator.Send(new DeactivateTenantCommand(id, request.Reason));
        return HandleResult(result);
    }
}

// ─── Request DTOs ─────────────────────────────────────

public sealed record UpdateTenantProfileRequest(
    string Name,
    string ContactEmail,
    string? AdminName = null,
    string? Description = null,
    string? LogoUrl = null,
    string? CustomDomain = null);

public sealed record UpdateSubscriptionRequest(
    SubscriptionTier Tier,
    DateTime? ExpiresOnUtc = null);

public sealed record ConfigureFeatureRequest(
    string FeatureCode,
    bool IsEnabled,
    DateTime? ExpiresOnUtc = null);

public sealed record SuspendRequest(string Reason);

public sealed record DeactivateRequest(string Reason);
