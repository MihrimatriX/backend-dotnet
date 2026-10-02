using System.Text.Json;
using EcommerceBackend.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace EcommerceBackend.Infrastructure.Caching;

/// <summary>
/// Redis üzerinden katalog önbelleği. Önbellek isteğe bağlıdır (§6): Redis'e ulaşılamazsa okuma ıska sayılır,
/// yazma/silme atlanır ve istek veritabanından cevaplanır.
/// </summary>
public sealed class RedisCatalogReadCache : ICatalogReadCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCatalogReadCache> _logger;

    public RedisCatalogReadCache(IDistributedCache cache, ILogger<RedisCatalogReadCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var bytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            return bytes is null || bytes.Length == 0 ? null : JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Katalog önbelleği okunamadı ({Key}); veritabanı kullanılacak.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            await _cache.SetAsync(
                key,
                bytes,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Katalog önbelleğine yazılamadı ({Key}).", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Katalog önbellek anahtarı silinemedi ({Key}).", key);
        }
    }
}
