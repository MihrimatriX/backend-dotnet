namespace EcommerceBackend.Application.Abstractions.Caching;

/// <summary>
/// Katalog okuma önbelleği (Redis üzerinden JSON).
/// </summary>
public interface ICatalogReadCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        where T : class;

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
