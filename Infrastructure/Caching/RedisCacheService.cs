using StackExchange.Redis;

namespace MyFirstApi.Infrastructure.Caching;

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MyFirstApi.Core.Ports.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer multiplexer,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _multiplexer = multiplexer;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);
            if (json is null) return default;

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            // Cache failure tidak boleh crash aplikasi — log dan lanjut
            _logger.LogWarning(ex, "[Cache] GET failed for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(key, json, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Cache] SET failed for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Cache] REMOVE failed for key: {Key}", key);
        }
    }

    // Untuk invalidate semua cache yang match pattern — misal "links:*"
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _multiplexer.GetServers().FirstOrDefault();
            if (server is null) return;

            var db = _multiplexer.GetDatabase();
            var keys = server.Keys(pattern: pattern);

            foreach (var key in keys)
                await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Cache] REMOVE BY PATTERN failed: {Pattern}", pattern);
        }
    }
}