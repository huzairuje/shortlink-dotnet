namespace MyFirstApi.Infrastructure.Persistence;

using MyFirstApi.Core.Entities;
using MyFirstApi.Core.Ports.Repositories;

// "Adapter" — implements port, tahu detail implementasi
public class InMemoryLinkRepository : ILinkRepository
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, Link> _store = new();

    public Task<Link?> GetBySlugAsync(string slug)
    {
        _lock.EnterReadLock();
        try
        {
            _store.TryGetValue(slug, out var link);
            return Task.FromResult(link);
        }
        finally { _lock.ExitReadLock(); }
    }

    public Task<IEnumerable<Link>> GetAllAsync()
    {
        _lock.EnterReadLock();
        try { return Task.FromResult<IEnumerable<Link>>(_store.Values.ToList()); }
        finally { _lock.ExitReadLock(); }
    }

    public Task SaveAsync(Link link)
    {
        _lock.EnterWriteLock();
        try { _store[link.Slug] = link; }
        finally { _lock.ExitWriteLock(); }
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string slug)
    {
        _lock.EnterWriteLock();
        try { return Task.FromResult(_store.Remove(slug)); }
        finally { _lock.ExitWriteLock(); }
    }

    public Task<bool> ExistsAsync(string slug)
    {
        _lock.EnterReadLock();
        try { return Task.FromResult(_store.ContainsKey(slug)); }
        finally { _lock.ExitReadLock(); }
    }

    // Dipanggil oleh JsonPersistenceService untuk seed data dari file
    public Task SeedAsync(IEnumerable<Link> links)
    {
        _lock.EnterWriteLock();
        try
        {
            foreach (var link in links)
                _store[link.Slug] = link;
        }
        finally { _lock.ExitWriteLock(); }
        return Task.CompletedTask;
    }
}