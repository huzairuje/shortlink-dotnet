namespace MyFirstApi.Api.Endpoints;

using MyFirstApi.Api.DTOs;
using MyFirstApi.Core.Entities;
using MyFirstApi.Core.Ports.Services;

public static class LinkEndpoints
{
    public static void MapLinkEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/links").WithOpenApi();

        group.MapPost("/", CreateLink).WithName("CreateLink");
        group.MapGet("/", GetAllLinks).WithName("GetAllLinks");
        group.MapGet("/{slug}", GetLink).WithName("GetLink");
        group.MapDelete("/{slug}", DeleteLink).WithName("DeleteLink");

        // Redirect endpoint di root — bukan di group /links
        app.MapGet("/{slug}", RedirectLink).WithName("RedirectLink");
    }

    private static async Task<IResult> CreateLink(
        CreateLinkRequest req,
        ILinkService service)
    {
        if (string.IsNullOrWhiteSpace(req.OriginalUrl))
            return Results.BadRequest(new { error = "originalUrl is required" });

        try
        {
            var link = await service.CreateLinkAsync(req.OriginalUrl, req.CustomSlug);
            return Results.Created($"/links/{link.Slug}", ToResponse(link));
        }
        catch (ArgumentException ex)
        {
            // Domain validation error → 422 Unprocessable Entity
            return Results.UnprocessableEntity(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Business rule violation (slug conflict) → 409 Conflict
            return Results.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetAllLinks(ILinkService service)
    {
        var links = await service.GetAllLinksAsync();
        return Results.Ok(links.Select(ToResponse));
    }

    private static async Task<IResult> GetLink(string slug, ILinkService service)
    {
        var link = await service.GetLinkAsync(slug);
        return link is null ? Results.NotFound() : Results.Ok(ToResponse(link));
    }

    private static async Task<IResult> DeleteLink(string slug, ILinkService service)
    {
        var deleted = await service.DeleteLinkAsync(slug);
        return deleted ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> RedirectLink(string slug, ILinkService service)
    {
        var link = await service.ClickAndGetLinkAsync(slug);
        return link is null ? Results.NotFound() : Results.Redirect(link.OriginalUrl);
    }

    // Mapping dari domain entity ke response DTO — jangan expose entity langsung
    private static LinkResponse ToResponse(Link link) => new(
        Slug: link.Slug,
        OriginalUrl: link.OriginalUrl,
        ShortUrl: $"http://localhost:5000/{link.Slug}",
        CreatedAt: link.CreatedAt,
        Clicks: link.Clicks
    );
}