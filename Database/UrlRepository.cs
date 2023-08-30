using Dapper;
using UrlShortener.Entities;

namespace UrlShortener.Database;

public class UrlRepository : IUrlRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UrlRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<DefaultIdType> CreateAsync(Url urlEntity)
    {
        const string sql =
            @"
                INSERT INTO ""Urls"" (""OriginalUrl"", ""ShortenedUrl"")
                VALUES               ( @OriginalUrl  ,  @ShortenedUrl  )
                RETURNING ""Id"";
            ";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, urlEntity);
    }

    public async Task<Url?> GetByShortenedUrlAsync(string shortenedUrl)
    {
        const string sql =
            @"
                SELECT ""OriginalUrl"", ""ShortenedUrl""
                FROM ""Urls""
                WHERE ""ShortenedUrl"" = @ShortenedUrl
            ";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Url?>(sql, new { ShortenedUrl = shortenedUrl });
    }

    public async Task<bool> ShortenedValueExists(string shortenedUrl, CancellationToken cancellationToken)
    {
        const string sql =
            @"
                SELECT EXISTS(SELECT 1 FROM ""Urls"" WHERE ""ShortenedUrl"" = @ShortenedUrl);
            ";

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(sql, new { ShortenedUrl = shortenedUrl });
    }
}