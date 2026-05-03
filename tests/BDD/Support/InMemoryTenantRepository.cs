using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Domain.Tenants;

namespace CleanArch.BddTests.Support;

/// <summary>
/// In-memory implementation of ITenantRepository for BDD tests.
/// No database required — fast, deterministic, isolated per scenario.
/// </summary>
public sealed class InMemoryTenantRepository : ITenantRepository
{
    private readonly List<Tenant> _tenants = [];

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = _tenants.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(tenant);
    }

    public Task<Tenant?> GetByIdentifierAsync(string identifier, CancellationToken ct = default)
    {
        var tenant = _tenants.FirstOrDefault(t => t.Identifier == identifier);
        return Task.FromResult(tenant);
    }

    public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Tenant>>(_tenants.ToList());
    }

    public Task<IReadOnlyList<Tenant>> GetByStatusAsync(TenantStatus status, CancellationToken ct = default)
    {
        var filtered = _tenants.Where(t => t.Status == status).ToList();
        return Task.FromResult<IReadOnlyList<Tenant>>(filtered);
    }

    public Task<bool> ExistsAsync(string identifier, CancellationToken ct = default)
    {
        return Task.FromResult(_tenants.Any(t => t.Identifier == identifier));
    }

    public void Add(Tenant tenant) => _tenants.Add(tenant);
    public void Update(Tenant tenant) { /* In-memory — already mutated */ }

    // Test helper
    public IReadOnlyList<Tenant> GetAll() => _tenants.AsReadOnly();
}
