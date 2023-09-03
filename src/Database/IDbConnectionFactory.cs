using System.Data;

namespace UrlShortener.Database;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}