namespace UrlShortener.BackgroundServices;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterBackgroundServices(this IServiceCollection services, IConfiguration configuration)
        =>
            services
                .Configure<HostOptions>(opt =>
                {
                    opt.ServicesStartConcurrently = true;
                })
                .AddSingleton<UrlShortenerBackgroundService>()
                .AddSingleton<IHostedService>(sp => sp.GetRequiredService<UrlShortenerBackgroundService>())
                .AddSingleton<IQueueSizeIncreaser>(sp => sp.GetRequiredService<UrlShortenerBackgroundService>());
}