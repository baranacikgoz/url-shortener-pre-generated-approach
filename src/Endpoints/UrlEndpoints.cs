using System.Collections.Concurrent;
using UrlShortener.Database;
using UrlShortener.Entities;
using UrlShortener.BackgroundServices;
using RabbitMQ.Client;
using Polly;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

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

    private static async Task<IResult> ShortenAsync(
        string url,
        IModel channel,
        IUrlRepository db,
        IDistributedCache cache,
        IQueueSizeIncreaser queueSizeIncreaser,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return Results.BadRequest($"The provided url ({url}) is not valid.");
        }

        string? shortenedUrl = await GetShortenedUrlFromCache(url, cache, cancellationToken);

        if (shortenedUrl is not null)
        {
            return Results.Ok(shortenedUrl);
        }

        // Maybe a shortenedUrl for this originalUrl exists in the db but expired from the cache.
        // So we need to check the db first.

        shortenedUrl = await db.GetShortenedUrlByOriginalUrlAsync(url, cancellationToken);
        if (shortenedUrl is not null)
        {
            return Results.Ok(shortenedUrl);
        }

        // Now we are sure that we need to generate a new shortenedUrl.
        shortenedUrl = await GenerateNewShortenedUrl(queueSizeIncreaser, channel);

        if (shortenedUrl is null) // Queue was empty even after retries.
        {
            return Results.Problem("We are having unexpected amount of requests right now. Please try again in a minute.", statusCode: 500);
        }

        await db.CreateAsync(new Url
        {
            OriginalUrl = url,
            ShortenedUrl = shortenedUrl
        }, cancellationToken);


        var cacheOptions = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromDays(15));

        await cache.SetStringAsync(url, shortenedUrl, cacheOptions, cancellationToken);

        return Results.Ok(shortenedUrl);
    }

    public static async Task<IResult> RedirectAsync(
        string shortenedUrl,
        IUrlRepository db,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(shortenedUrl, UriKind.Absolute, out var _))
        {
            return Results.BadRequest($"The provided url ({shortenedUrl}) is not valid.");
        }

        string? url = await db.GetOriginalUrlByShortenedUrlAsync(shortenedUrl, cancellationToken);

        if (url is null)
        {
            return Results.BadRequest($"The url not found for the shortened url ({shortenedUrl}).");
        }

        return Results.Redirect(url);
    }

    private static async Task<string?> GetShortenedUrlFromCache(string url, IDistributedCache cache, CancellationToken cancellationToken)
    {
        var cacheRetryPolicy = Policy
                .Handle<Exception>()
                .OrResult<string?>(r => r is null)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 100),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        // Log the exception or perform other actions
                    });

        string? shortenedUrl = await cacheRetryPolicy.ExecuteAsync(() => cache.GetStringAsync(url, cancellationToken));
        return shortenedUrl;
    }

    private static async Task<string?> GenerateNewShortenedUrl(IQueueSizeIncreaser queueSizeIncreaser, IModel channel)
    {
        var retryPolicy = Policy
            .HandleResult<BasicGetResult>(r => r is null)
            .WaitAndRetryAsync(
                retryCount: 3, // Number of retries
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 300),
                onRetry: (result, timeSpan, retryCount, context) =>
                {
                    int increaseByPercentage = retryCount * 5;
                    queueSizeIncreaser.IncreaseQueueSize(increaseByPercentage);
                });

        var result = await retryPolicy.ExecuteAsync(() => Task.FromResult(channel.BasicGet(queue: UrlShortenerBackgroundService.QueueName, autoAck: true)));

        if (result is null)
        {
            return null;
        }

        return Encoding.UTF8.GetString(result.Body.ToArray());
    }

}