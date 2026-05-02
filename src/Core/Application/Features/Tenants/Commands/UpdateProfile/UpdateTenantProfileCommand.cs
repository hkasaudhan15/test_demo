using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.Tenants.Commands.UpdateProfile;

public sealed record UpdateTenantProfileCommand(
    Guid TenantId,
    string Name,
    string ContactEmail,
    string? AdminName = null,
    string? Description = null,
    string? LogoUrl = null,
    string? CustomDomain = null) : ICommand;
