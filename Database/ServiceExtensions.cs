namespace UrlShortener.Database;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterDatabaseAndRepoistories(this IServiceCollection services, IConfiguration configuration)
        =>
            services
                .AddSingleton<IDbConnectionFactory>(new PostgresConnectionFactory(configuration.GetConnectionString("Postgres") ?? throw new ApplicationException("Postgres connection string is not found!")))
                .AddSingleton<IUrlRepository, UrlRepository>();
}