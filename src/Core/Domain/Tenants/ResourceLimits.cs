namespace CleanArch.Domain.Tenants;

/// <summary>
/// Resource limits per tenant — enforced at the application layer.
/// Immutable value object representing plan-based resource quotas.
/// </summary>
public sealed record ResourceLimits
{
    public int MaxUsers { get; init; }
    public int MaxStorageMb { get; init; }
    public int MaxApiCallsPerDay { get; init; }
    public int MaxProjectsOrWorkspaces { get; init; }

    private ResourceLimits() { }

    public ResourceLimits(int maxUsers, int maxStorageMb, int maxApiCallsPerDay, int maxProjectsOrWorkspaces)
    {
        if (maxUsers < 0) throw new ArgumentException("MaxUsers cannot be negative.", nameof(maxUsers));
        if (maxStorageMb < 0) throw new ArgumentException("MaxStorageMb cannot be negative.", nameof(maxStorageMb));

        MaxUsers = maxUsers;
        MaxStorageMb = maxStorageMb;
        MaxApiCallsPerDay = maxApiCallsPerDay;
        MaxProjectsOrWorkspaces = maxProjectsOrWorkspaces;
    }

    public static ResourceLimits ForTier(SubscriptionTier tier) => tier switch
    {
        SubscriptionTier.Free => new(5, 500, 1_000, 3),
        SubscriptionTier.Starter => new(25, 5_000, 10_000, 20),
        SubscriptionTier.Professional => new(100, 50_000, 100_000, 100),
        SubscriptionTier.Enterprise => new(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue),
        _ => new(5, 500, 1_000, 3)
    };
}
