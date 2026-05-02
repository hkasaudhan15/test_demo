using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants;

public sealed record TenantResponse(
    Guid Id,
    string Identifier,
    string Name,
    string? Description,
    string ContactEmail,
    string? AdminName,
    TenantStatus Status,
    SubscriptionTier Tier,
    string? LogoUrl,
    string? CustomDomain,
    int MaxUsers,
    int MaxStorageMb,
    int MaxApiCallsPerDay,
    int MaxProjectsOrWorkspaces,
    DateTime? ActivatedOnUtc,
    DateTime? SubscriptionExpiresOnUtc,
    DateTime CreatedOnUtc,
    IReadOnlyList<TenantFeatureResponse> Features);

public sealed record TenantFeatureResponse(
    string FeatureCode,
    bool IsEnabled,
    DateTime? ExpiresOnUtc);
