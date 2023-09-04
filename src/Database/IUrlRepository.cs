using UrlShortener.Entities;

namespace UrlShortener.Database;

public interface IUrlRepository
{
    Task CreateAsync(Url urlEntity, CancellationToken cancellationToken);

    Task<string?> GetOriginalUrlByShortenedUrlAsync(string shortenedUrl, CancellationToken cancellationToken);
    Task<string?> GetShortenedUrlByOriginalUrlAsync(string url, CancellationToken cancellationToken);
    Task<bool> ShortenedUrlExists(string shortenedValue, CancellationToken cancellationToken);
}