using CleanArch.Domain.Primitives.Results;
using CleanArch.Domain.Tenants;

namespace CleanArch.BddTests.Support;

/// <summary>
/// Shared test context for passing state between step definitions.
/// Scoped per scenario via SpecFlow's dependency injection.
/// </summary>
public sealed class TestContext
{
    // ─── Tenant Under Test ───────────────────────────────
    public Tenant? CurrentTenant { get; set; }
    public List<Tenant> TenantCollection { get; set; } = [];

    // ─── Results ─────────────────────────────────────────
    public Result? LastResult { get; set; }
    public Exception? LastException { get; set; }
    public Guid? LastCreatedId { get; set; }

    // ─── Command/Query Inputs ────────────────────────────
    public string? InputIdentifier { get; set; }
    public string? InputName { get; set; }
    public string? InputEmail { get; set; }
    public string? InputAdminName { get; set; }
    public string? InputDescription { get; set; }
    public SubscriptionTier InputTier { get; set; } = SubscriptionTier.Free;
    public string? InputReason { get; set; }
    public string? InputFeatureCode { get; set; }
    public bool InputFeatureEnabled { get; set; }
    public DateTime? InputExpiresOnUtc { get; set; }
}
