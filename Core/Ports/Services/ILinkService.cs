namespace MyFirstApi.Core.Ports.Services;

using Entities;

public interface ILinkService
{
    Task<Link> CreateLinkAsync(string originalUrl, string? customSlug = null);
    Task<Link?> GetLinkAsync(string slug);
    Task<IEnumerable<Link>> GetAllLinksAsync();
    Task<Link?> ClickAndGetLinkAsync(string slug);
    Task<bool> DeleteLinkAsync(string slug);
}