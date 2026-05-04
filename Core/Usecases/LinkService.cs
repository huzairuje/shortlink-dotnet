namespace MyFirstApi.Core.UseCases;

using MyFirstApi.Core.Entities;
using MyFirstApi.Core.Ports.Repositories;
using MyFirstApi.Core.Ports.Services;

// UseCase — hanya depends pada port (interface), tidak tahu infrastructure
public class LinkService : ILinkService
{
    private readonly ILinkRepository _repository;

    public LinkService(ILinkRepository repository)
    {
        _repository = repository;
    }

    public async Task<Link> CreateLinkAsync(string originalUrl, string? customSlug = null)
    {
        // Normalize — treat empty string sama seperti null
        var slug = string.IsNullOrWhiteSpace(customSlug)
            ? GenerateSlug()
            : customSlug.Trim();

        if (await _repository.ExistsAsync(slug))
            throw new InvalidOperationException($"Slug '{slug}' already exists");

        var link = Link.Create(slug, originalUrl);
        await _repository.SaveAsync(link);
        return link;
    }

    public Task<Link?> GetLinkAsync(string slug) =>
        _repository.GetBySlugAsync(slug);

    public Task<IEnumerable<Link>> GetAllLinksAsync() =>
        _repository.GetAllAsync();

    public async Task<Link?> ClickAndGetLinkAsync(string slug)
    {
        var link = await _repository.GetBySlugAsync(slug);
        if (link is null) return null;

        link.RecordClick();
        await _repository.SaveAsync(link);
        return link;
    }

    public Task<bool> DeleteLinkAsync(string slug) =>
        _repository.DeleteAsync(slug);

    private static string GenerateSlug()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}