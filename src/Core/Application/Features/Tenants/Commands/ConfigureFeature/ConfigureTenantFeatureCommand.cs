using CleanArch.Application.Abstractions.Messaging.Commands;

namespace CleanArch.Application.Features.Tenants.Commands.ConfigureFeature;

public sealed record ConfigureTenantFeatureCommand(
    Guid TenantId,
    string FeatureCode,
    bool IsEnabled,
    DateTime? ExpiresOnUtc = null) : ICommand;
