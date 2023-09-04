using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;
using UrlShortener.Database;

namespace UrlShortener.BackgroundServices;

public class UrlShortenerBackgroundService : IHostedService, IQueueSizeIncreaser
{
    private readonly string _domainNameOfTheSystem;
    private readonly int _shortenedValueLength;
    private uint _queueSize; // We want to keep our message queue at this size.
    private readonly int _waitForMsIfQueueFull;
    private readonly IUrlRepository _urlRepository;
    private readonly IModel _channel;
    public const string QueueName = "shortened-urls";
    private readonly Random _random = new();
    private const string _alphabet = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public UrlShortenerBackgroundService(IConfiguration configuration, IUrlRepository urlRepository, IModel channel)
    {
        _domainNameOfTheSystem = configuration.GetValue<string>("DomainNameOfTheSystem") ?? throw new ApplicationException("DomainNameOfTheSystem is not found!");
        _shortenedValueLength = configuration.GetValue<int>("ShortenedLength");
        _queueSize = configuration.GetValue<uint>("MaxQueueSize");
        _waitForMsIfQueueFull = configuration.GetValue<int>("WaitForMsIfQueueFull");
        _urlRepository = urlRepository;

        // Set up the queue.
        _channel = channel;
        DeclareQueue(_channel);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!ShouldEnqueue())
            {
                await Task.Delay(_waitForMsIfQueueFull, cancellationToken);
                continue;
            }

            string uniqueRandomString;
            string shortenedUrl;
            do
            {
                uniqueRandomString = GenerateRandomString();
                shortenedUrl = $"https://{_domainNameOfTheSystem}/{uniqueRandomString}";
            }
            while (await _urlRepository.ShortenedUrlExists(shortenedUrl, cancellationToken)
                    && !cancellationToken.IsCancellationRequested);

            var body = Encoding.UTF8.GetBytes(shortenedUrl);
            _channel.BasicPublish(exchange: "", routingKey: QueueName, body: body, basicProperties: null);
        }
    }

    private string GenerateRandomString()
    {
        char[] chars = new char[_shortenedValueLength];

        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = _alphabet[_random.Next(0, _alphabet.Length)];
        }

        return new string(chars);
    }

    public void IncreaseQueueSize(int byPercentage)
    {
        uint currentQueueSize = GetCurrentQueueSize();
        uint increasedAmount = currentQueueSize + (currentQueueSize * (uint)byPercentage / 100);

        _queueSize = increasedAmount;
    }

    private static QueueDeclareOk DeclareQueue(IModel channel)
        => channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);


    private bool ShouldEnqueue() => GetCurrentQueueSize() <= _queueSize;

    private uint GetCurrentQueueSize() => DeclareQueue(_channel).MessageCount;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}