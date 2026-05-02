using CleanArch.Domain.Tenants;

namespace CleanArch.Application.Abstractions.Tenants;

/// <summary>
/// Repository for the Tenant aggregate root.
/// Separate from generic IRepository because Tenant is not tenant-scoped
/// (it IS the tenant entity — lives above tenant isolation).
/// </summary>
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetByIdentifierAsync(string identifier, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetByStatusAsync(TenantStatus status, CancellationToken ct = default);
    Task<bool> ExistsAsync(string identifier, CancellationToken ct = default);
    void Add(Tenant tenant);
    void Update(Tenant tenant);
}
