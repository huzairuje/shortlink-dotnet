namespace MyFirstApi.Core.Entities;

public class Link
{
    public string Slug { get; private set; }
    public string OriginalUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int Clicks { get; private set; }

    // Private constructor — hanya bisa dibuat lewat factory method
    private Link() { }

    public static Link Create(string slug, string originalUrl)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty");
        if (string.IsNullOrWhiteSpace(originalUrl))
            throw new ArgumentException("Original URL cannot be empty");

        return new Link
        {
            Slug = slug,
            OriginalUrl = originalUrl,
            CreatedAt = DateTime.UtcNow,
            Clicks = 0
        };
    }

    // Domain behavior — logic ada di entity, bukan di service
    public void RecordClick() => Clicks++;
}