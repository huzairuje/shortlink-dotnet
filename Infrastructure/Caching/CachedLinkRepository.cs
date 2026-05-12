namespace MyFirstApi.Infrastructure.Caching;

using MyFirstApi.Core.Entities;
using MyFirstApi.Core.Ports.Caching;
using MyFirstApi.Core.Ports.Repositories;

public class CachedLinkRepository : ILinkRepository
{
    private readonly ILinkRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedLinkRepository> _logger;

    // Cache key constants — centralized supaya tidak typo
    private const string AllLinksKey = "links:all";
    private static string SlugKey(string slug) => $"links:slug:{slug}";

    public CachedLinkRepository(
        ILinkRepository inner,
        ICacheService cache,
        ILogger<CachedLinkRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Link?> GetBySlugAsync(string slug)
    {
        var key = SlugKey(slug);

        // Cache hit
        var cached = await _cache.GetAsync<Link>(key);
        if (cached is not null)
        {
            _logger.LogDebug("[Cache] HIT for key: {Key}", key);
            return cached;
        }

        // Cache miss — ambil dari repository
        _logger.LogDebug("[Cache] MISS for key: {Key}", key);
        var link = await _inner.GetBySlugAsync(slug);

        if (link is not null)
            await _cache.SetAsync(key, link, TimeSpan.FromMinutes(10));

        return link;
    }

    public async Task<IEnumerable<Link>> GetAllAsync()
    {
        var cached = await _cache.GetAsync<List<Link>>(AllLinksKey);
        if (cached is not null)
        {
            _logger.LogDebug("[Cache] HIT for key: {Key}", AllLinksKey);
            return cached;
        }

        _logger.LogDebug("[Cache] MISS for key: {Key}", AllLinksKey);
        var links = await _inner.GetAllAsync();
        var list = links.ToList();

        await _cache.SetAsync(AllLinksKey, list, TimeSpan.FromMinutes(5));
        return list;
    }

    public async Task SaveAsync(Link link)
    {
        await _inner.SaveAsync(link);

        // Invalidate cache — data berubah
        await _cache.RemoveAsync(SlugKey(link.Slug));
        await _cache.RemoveAsync(AllLinksKey);
    }

    public async Task<bool> DeleteAsync(string slug)
    {
        var result = await _inner.DeleteAsync(slug);

        if (result)
        {
            await _cache.RemoveAsync(SlugKey(slug));
            await _cache.RemoveAsync(AllLinksKey);
        }

        return result;
    }

    public Task<bool> ExistsAsync(string slug) =>
        _inner.ExistsAsync(slug);
}