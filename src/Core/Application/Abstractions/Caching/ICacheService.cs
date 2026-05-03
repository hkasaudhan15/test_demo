namespace CleanArch.Application.Abstractions.Caching;

/// <summary>
/// Distributed cache abstraction — supports Redis, MemoryCache, or any backend.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefixKey, CancellationToken ct = default);
}
