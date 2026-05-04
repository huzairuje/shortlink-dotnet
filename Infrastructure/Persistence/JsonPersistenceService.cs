namespace MyFirstApi.Infrastructure.Persistence;

using System.Text.Json;
using MyFirstApi.Core.Entities;

public class JsonPersistenceService
{
    private readonly InMemoryLinkRepository _repository;
    private const string FilePath = "links.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    // Pindah ke sini — private record di dalam class, bukan file-local
    private record LinkSnapshot(
        string Slug,
        string OriginalUrl,
        DateTime CreatedAt,
        int Clicks
    );

    public JsonPersistenceService(InMemoryLinkRepository repository)
    {
        _repository = repository;
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(FilePath)) return;

        var json = await File.ReadAllTextAsync(FilePath);
        var snapshots = JsonSerializer.Deserialize<List<LinkSnapshot>>(json, Options);
        if (snapshots is null) return;

        var links = snapshots.Select(RestoreEntity);
        await _repository.SeedAsync(links);

        Console.WriteLine($"[Persistence] Restored {snapshots.Count} links from {FilePath}");
    }

    public async Task SaveAsync()
    {
        var links = await _repository.GetAllAsync();
        var snapshots = links.Select(l => new LinkSnapshot(
            l.Slug, l.OriginalUrl, l.CreatedAt, l.Clicks
        )).ToList();

        var json = JsonSerializer.Serialize(snapshots, Options);
        await File.WriteAllTextAsync(FilePath, json);
        Console.WriteLine($"[Persistence] Saved {snapshots.Count} links to {FilePath}");
    }

    private static Link RestoreEntity(LinkSnapshot s)
    {
        var link = Link.Create(s.Slug, s.OriginalUrl);

        typeof(Link).GetProperty(nameof(Link.CreatedAt))!
            .SetValue(link, s.CreatedAt);
        typeof(Link).GetProperty(nameof(Link.Clicks))!
            .SetValue(link, s.Clicks);

        return link;
    }
}