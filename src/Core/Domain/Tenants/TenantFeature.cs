using CleanArch.Domain.Abstractions.Entities;

namespace CleanArch.Domain.Tenants;

/// <summary>
/// Feature flag for a tenant — controls access to specific features.
/// Owned by the Tenant aggregate (not an aggregate root itself).
///
/// Features can be enabled/disabled per tenant to support:
///   - Gradual rollouts (enable feature for specific tenants)
///   - Subscription-gated features
///   - Beta/preview features
///   - Kill switches
/// </summary>
public sealed class TenantFeature : Entity
{
    public Guid TenantId { get; private set; }
    public string FeatureCode { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public DateTime? ExpiresOnUtc { get; private set; }

    private TenantFeature() { } // EF Core

    internal TenantFeature(Guid id, Guid tenantId, string featureCode, bool isEnabled, DateTime? expiresOnUtc)
        : base(id)
    {
        TenantId = tenantId;
        FeatureCode = featureCode;
        IsEnabled = isEnabled;
        ExpiresOnUtc = expiresOnUtc;
    }

    public void Enable(DateTime? expiresOnUtc = null)
    {
        IsEnabled = true;
        ExpiresOnUtc = expiresOnUtc;
    }

    public void Disable()
    {
        IsEnabled = false;
        ExpiresOnUtc = null;
    }

    public bool IsActiveAt(DateTime utcNow)
    {
        return IsEnabled && (ExpiresOnUtc is null || ExpiresOnUtc > utcNow);
    }
}

/// <summary>
/// Well-known feature codes for compile-time safety.
/// </summary>
public static class FeatureCodes
{
    public const string AdvancedReporting = "advanced_reporting";
    public const string ApiAccess = "api_access";
    public const string AuditLog = "audit_log";
    public const string BulkOperations = "bulk_operations";
    public const string CustomBranding = "custom_branding";
    public const string DataExport = "data_export";
    public const string MultiLanguage = "multi_language";
    public const string PrioritySupport = "priority_support";
    public const string SsoIntegration = "sso_integration";
    public const string WebhookIntegration = "webhook_integration";
}
