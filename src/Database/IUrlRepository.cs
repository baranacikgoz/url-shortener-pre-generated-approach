using UrlShortener.Entities;

namespace UrlShortener.Database;

public interface IUrlRepository
{
    Task<DefaultIdType> CreateAsync(Url urlEntity);

    Task<Url?> GetByShortenedUrlAsync(string shortenedValue);

    Task<bool> ShortenedValueExists(string shortenedValue, CancellationToken cancellationToken);
}