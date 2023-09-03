using RabbitMQ.Client;

namespace UrlShortener.MessageQueue;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterRabbitMq(this IServiceCollection services, IConfiguration configuration)
        =>
            services
                .AddSingleton(sp =>
                {
                    string host = configuration.GetValue<string>("MessageQueueSettings:Host") ?? throw new ApplicationException("MessageQueueSettings:Host is not found!");
                    int port = configuration.GetValue<int>("MessageQueueSettings:Port");

                    var factory = new ConnectionFactory() { HostName = host, Port = port };
                    return factory.CreateConnection();
                })
                .AddSingleton(sp =>
                {
                    var connection = sp.GetRequiredService<IConnection>();
                    return connection.CreateModel();
                });
}