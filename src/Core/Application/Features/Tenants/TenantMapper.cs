using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Features.Tenants;

internal static class TenantMapper
{
    internal static TenantResponse MapToResponse(Tenant tenant)
    {
        return new TenantResponse(
            tenant.Id,
            tenant.Identifier,
            tenant.Name,
            tenant.Description,
            tenant.ContactEmail,
            tenant.AdminName,
            tenant.Status,
            tenant.Tier,
            tenant.LogoUrl,
            tenant.CustomDomain,
            tenant.MaxUsers,
            tenant.MaxStorageMb,
            tenant.MaxApiCallsPerDay,
            tenant.MaxProjectsOrWorkspaces,
            tenant.ActivatedOnUtc,
            tenant.SubscriptionExpiresOnUtc,
            tenant.CreatedOnUtc,
            tenant.Features
                .Select(f => new TenantFeatureResponse(f.FeatureCode, f.IsEnabled, f.ExpiresOnUtc))
                .ToList());
    }
}
