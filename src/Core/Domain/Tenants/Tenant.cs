using CleanArch.Domain.Abstractions.Entities;
using CleanArch.Domain.Tenants.Events;

namespace CleanArch.Domain.Tenants;

/// <summary>
/// Tenant aggregate root — manages the full tenant lifecycle.
///
/// Lifecycle: Provisioning → Active → Suspended/Deactivated → Active (reactivation)
///
/// Features:
///   - Unique identifier (slug-like, e.g., "acme-corp") — used in headers/routing
///   - Display name, contact email, admin contact
///   - Subscription tier with resource limits
///   - Feature flags (per-tenant feature gating)
///   - Connection string for DB-per-tenant pattern (optional)
///   - Domain events raised on all lifecycle transitions
///   - Soft-deletable, auditable, concurrency-protected (AggregateRoot)
/// </summary>
public sealed class Tenant : AggregateRoot
{
    private readonly List<TenantFeature> _features = [];

    public string Identifier { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ContactEmail { get; private set; } = string.Empty;
    public string? AdminName { get; private set; }
    public TenantStatus Status { get; private set; }
    public SubscriptionTier Tier { get; private set; }
    public string? ConnectionString { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? CustomDomain { get; private set; }

    // ─── Resource Limits (owned value object) ────────────
    public int MaxUsers { get; private set; }
    public int MaxStorageMb { get; private set; }
    public int MaxApiCallsPerDay { get; private set; }
    public int MaxProjectsOrWorkspaces { get; private set; }

    public DateTime? ActivatedOnUtc { get; private set; }
    public DateTime? SuspendedOnUtc { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateTime? SubscriptionExpiresOnUtc { get; private set; }

    public IReadOnlyList<TenantFeature> Features => _features.AsReadOnly();

    private Tenant() { } // EF Core

    private Tenant(
        Guid id,
        string identifier,
        string name,
        string contactEmail,
        string? adminName,
        string? description,
        SubscriptionTier tier) : base(id)
    {
        Identifier = identifier;
        Name = name;
        ContactEmail = contactEmail;
        AdminName = adminName;
        Description = description;
        Status = TenantStatus.Provisioning;
        Tier = tier;

        var limits = ResourceLimits.ForTier(tier);
        MaxUsers = limits.MaxUsers;
        MaxStorageMb = limits.MaxStorageMb;
        MaxApiCallsPerDay = limits.MaxApiCallsPerDay;
        MaxProjectsOrWorkspaces = limits.MaxProjectsOrWorkspaces;
    }

    // ─── Factory ─────────────────────────────────────────

    public static Tenant Register(
        string identifier,
        string name,
        string contactEmail,
        string? adminName = null,
        string? description = null,
        SubscriptionTier tier = SubscriptionTier.Free)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Tenant identifier is required.", nameof(identifier));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new ArgumentException("Contact email is required.", nameof(contactEmail));

        var tenant = new Tenant(
            Guid.NewGuid(),
            identifier.Trim().ToLowerInvariant(),
            name.Trim(),
            contactEmail.Trim().ToLowerInvariant(),
            adminName?.Trim(),
            description?.Trim(),
            tier);

        tenant.RaiseDomainEvent(new TenantRegisteredDomainEvent(
            tenant.Id, tenant.Identifier, tenant.Name, tenant.Tier));

        return tenant;
    }

    // ─── Lifecycle ───────────────────────────────────────

    public void Activate()
    {
        if (Status == TenantStatus.Active)
            return;

        Status = TenantStatus.Active;
        ActivatedOnUtc = DateTime.UtcNow;
        SuspensionReason = null;
        SuspendedOnUtc = null;

        RaiseDomainEvent(new TenantActivatedDomainEvent(Id, Identifier));
    }

    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Suspension reason is required.", nameof(reason));

        Status = TenantStatus.Suspended;
        SuspendedOnUtc = DateTime.UtcNow;
        SuspensionReason = reason.Trim();
    }

    public void Deactivate(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Deactivation reason is required.", nameof(reason));

        Status = TenantStatus.Deactivated;
        SuspensionReason = reason.Trim();

        RaiseDomainEvent(new TenantDeactivatedDomainEvent(Id, Identifier, reason));
    }

    // ─── Profile ─────────────────────────────────────────

    public void UpdateProfile(string name, string? description, string contactEmail, string? adminName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new ArgumentException("Contact email is required.", nameof(contactEmail));

        Name = name.Trim();
        Description = description?.Trim();
        ContactEmail = contactEmail.Trim().ToLowerInvariant();
        AdminName = adminName?.Trim();
    }

    public void SetCustomDomain(string? domain)
    {
        CustomDomain = domain?.Trim().ToLowerInvariant();
    }

    public void SetLogoUrl(string? logoUrl)
    {
        LogoUrl = logoUrl?.Trim();
    }

    public void SetConnectionString(string? connectionString)
    {
        ConnectionString = connectionString;
    }

    // ─── Subscription ────────────────────────────────────

    public void ChangeSubscription(SubscriptionTier newTier, DateTime? expiresOnUtc = null)
    {
        var oldTier = Tier;
        Tier = newTier;
        SubscriptionExpiresOnUtc = expiresOnUtc;

        var limits = ResourceLimits.ForTier(newTier);
        MaxUsers = limits.MaxUsers;
        MaxStorageMb = limits.MaxStorageMb;
        MaxApiCallsPerDay = limits.MaxApiCallsPerDay;
        MaxProjectsOrWorkspaces = limits.MaxProjectsOrWorkspaces;

        if (oldTier != newTier)
            RaiseDomainEvent(new TenantSubscriptionChangedDomainEvent(Id, oldTier, newTier));
    }

    // ─── Feature Flags ───────────────────────────────────

    public void EnableFeature(string featureCode, DateTime? expiresOnUtc = null)
    {
        if (string.IsNullOrWhiteSpace(featureCode))
            throw new ArgumentException("Feature code is required.", nameof(featureCode));

        var existing = _features.FirstOrDefault(f => f.FeatureCode == featureCode);
        if (existing is not null)
        {
            existing.Enable(expiresOnUtc);
        }
        else
        {
            _features.Add(new TenantFeature(Guid.NewGuid(), Id, featureCode, true, expiresOnUtc));
        }
    }

    public void DisableFeature(string featureCode)
    {
        var existing = _features.FirstOrDefault(f => f.FeatureCode == featureCode);
        existing?.Disable();
    }

    public bool HasFeature(string featureCode)
    {
        var feature = _features.FirstOrDefault(f => f.FeatureCode == featureCode);
        return feature is not null && feature.IsActiveAt(DateTime.UtcNow);
    }

    // ─── Queries ─────────────────────────────────────────

    public bool IsActive => Status == TenantStatus.Active;
    public bool IsSubscriptionExpired => SubscriptionExpiresOnUtc.HasValue && SubscriptionExpiresOnUtc < DateTime.UtcNow;
}
