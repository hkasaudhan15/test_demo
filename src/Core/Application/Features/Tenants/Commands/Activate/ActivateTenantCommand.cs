using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.Tenants.Commands.Activate;

public sealed record ActivateTenantCommand(Guid TenantId) : ICommand;
