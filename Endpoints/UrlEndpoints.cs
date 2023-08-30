using System.Collections.Concurrent;
using UrlShortener.Database;
using UrlShortener.Entities;

namespace UrlShortener.Endpoints;

public static class UrlEndpoints
{
    public static WebApplication MapUrlEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("urls");
        group.MapPost("shorten", ShortenAsync);
        group.MapGet("redirect", RedirectAsync);

        return app;
    }

    private static async Task<IResult> ShortenAsync(string url, ConcurrentQueue<string> shortenedUrls, IUrlRepository repository)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return Results.BadRequest($"The provided url ({url}) is not valid.");
        }

        if (!shortenedUrls.TryDequeue(out var shortenedUrl))
        {
            return Results.Problem("We are having unexpected amount of requests right now. Please try again in a minute.", statusCode: 500);
        }

        await repository.CreateAsync(new Url
        {
            OriginalUrl = url,
            ShortenedUrl = shortenedUrl
        });

        return Results.Ok(shortenedUrl);
    }

    private static async Task<IResult> RedirectAsync(string shortenedUrl, IUrlRepository repository)
    {
        if (!Uri.TryCreate(shortenedUrl, UriKind.Absolute, out var shortenedUri))
        {
            return Results.BadRequest($"The provided url ({shortenedUrl}) is not valid.");
        }

        Url? url = await repository.GetByShortenedUrlAsync(shortenedUrl);

        if (url is null)
        {
            return Results.BadRequest($"The url not found for the shortened url ({shortenedUrl}).");
        }

        return Results.Redirect(url.OriginalUrl);
    }
}