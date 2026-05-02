namespace CleanArch.CrossCutting.MultiTenancy.Abstractions;

/// <summary>
/// Persistent store for tenant information.
/// Used by ITenantResolver to look up tenant details from the database.
/// Implemented in the Infrastructure layer using EF Core.
/// </summary>
public interface ITenantStore
{
    Task<TenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken ct = default);
}
