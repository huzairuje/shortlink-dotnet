namespace MyFirstApi.Api.DTOs;

// Request/Response DTOs — dipisah dari entity supaya API contract bisa evolve sendiri
public record CreateLinkRequest(
    string OriginalUrl,
    string? CustomSlug
);

public record LinkResponse(
    string Slug,
    string OriginalUrl,
    string ShortUrl,
    DateTime CreatedAt,
    int Clicks
);