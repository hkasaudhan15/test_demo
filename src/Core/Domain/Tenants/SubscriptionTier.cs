namespace CleanArch.Domain.Tenants;

/// <summary>
/// Subscription tiers — controls feature availability and resource limits.
/// </summary>
public enum SubscriptionTier
{
    Free = 0,
    Starter = 1,
    Professional = 2,
    Enterprise = 3
}
