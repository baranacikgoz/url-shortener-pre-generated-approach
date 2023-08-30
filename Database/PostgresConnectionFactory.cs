using Npgsql;
using System.Data;

namespace UrlShortener.Database;

public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Postgres connection string not found.");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}