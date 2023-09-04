using StackExchange.Redis;

namespace UrlShortener.Caching;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterRedis(this IServiceCollection services, IConfiguration configuration)
        =>
        services
            .AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis") ?? throw new ApplicationException("Redis connection string not found");
                options.InstanceName = "UrlShortener_";
            });


}