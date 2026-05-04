namespace MyFirstApi.Core.Ports.Repositories;

using MyFirstApi.Core.Entities;

// Ini "port" — interface murni, tidak tahu implementasinya pakai apa
public interface ILinkRepository
{
    Task<Link?> GetBySlugAsync(string slug);
    Task<IEnumerable<Link>> GetAllAsync();
    Task SaveAsync(Link link);
    Task<bool> DeleteAsync(string slug);
    Task<bool> ExistsAsync(string slug);
}