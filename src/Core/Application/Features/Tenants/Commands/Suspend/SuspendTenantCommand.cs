using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.Tenants.Commands.Suspend;

public sealed record SuspendTenantCommand(Guid TenantId, string Reason) : ICommand;
