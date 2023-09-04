using Dapper;
using UrlShortener.Entities;

namespace UrlShortener.Database;

public class UrlRepository : IUrlRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UrlRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task CreateAsync(Url urlEntity, CancellationToken cancellationToken)
    {
        const string sql =
            @"
                INSERT INTO ""Urls"" (""OriginalUrl"", ""ShortenedUrl"")
                VALUES               ( @OriginalUrl  ,  @ShortenedUrl  )
            ";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, urlEntity);
    }

    public async Task<string?> GetOriginalUrlByShortenedUrlAsync(string shortenedUrl, CancellationToken cancellationToken)
    {
        const string sql =
            @"
                SELECT ""OriginalUrl""
                FROM ""Urls""
                WHERE ""ShortenedUrl"" = @ShortenedUrl
            ";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<string?>(sql, new { ShortenedUrl = shortenedUrl });
    }

    public async Task<string?> GetShortenedUrlByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken)
    {
        const string sql =
            @"
                SELECT ""ShortenedUrl""
                FROM ""Urls""
                WHERE ""OriginalUrl"" = @OriginalUrl
            ";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<string?>(sql, new { OriginalUrl = originalUrl });
    }

    public async Task<bool> ShortenedUrlExists(string shortenedUrl, CancellationToken cancellationToken)
    {
        const string sql =
            @"
                SELECT EXISTS(SELECT 1 FROM ""Urls"" WHERE ""ShortenedUrl"" = @ShortenedUrl);
            ";

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(sql, new { ShortenedUrl = shortenedUrl });
    }
}