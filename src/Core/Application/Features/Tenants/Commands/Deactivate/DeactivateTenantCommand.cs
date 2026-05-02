using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.Tenants.Commands.Deactivate;

public sealed record DeactivateTenantCommand(Guid TenantId, string Reason) : ICommand;
