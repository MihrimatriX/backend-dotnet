using System.Text.Json;
using EcommerceBackend.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace EcommerceBackend.Infrastructure.Caching;

public sealed class RedisCatalogReadCache : ICatalogReadCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;

    public RedisCatalogReadCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var bytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (bytes is null || bytes.Length == 0)
            return null;

        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await _cache.SetAsync(
            key,
            bytes,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            cancellationToken).ConfigureAwait(false);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(key, cancellationToken);
}
