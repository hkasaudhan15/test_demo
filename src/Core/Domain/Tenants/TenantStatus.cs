namespace CleanArch.Domain.Tenants;

/// <summary>
/// Tenant lifecycle states.
///
/// Provisioning → Active → Suspended → Active (reactivation)
///                Active → Deactivated (terminal, can be reactivated by admin)
/// </summary>
public enum TenantStatus
{
    Provisioning = 0,
    Active = 1,
    Suspended = 2,
    Deactivated = 3
}
