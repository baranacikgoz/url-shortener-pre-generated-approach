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
    private int _maxQueueSize;
    private uint _currentQueueSize = 0;
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
        _maxQueueSize = configuration.GetValue<int>("MaxQueueSize");
        _waitForMsIfQueueFull = configuration.GetValue<int>("WaitForMsIfQueueFull");
        _urlRepository = urlRepository;

        // Set up the queue.
        _channel = channel;
        DeclareQueueAndUpdateCurrentQueueSize();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!QueueAcceptsEntries())
            {
                await Task.Delay(_waitForMsIfQueueFull, cancellationToken);
                continue;
            }

            string uniqueShortenedValue;
            do
            {
                uniqueShortenedValue = GenerateRandomShortenedValue();
            }
            while (await _urlRepository.ShortenedValueExists(uniqueShortenedValue, cancellationToken)
                    && !cancellationToken.IsCancellationRequested);

            string shortenedUrl = $"https://{_domainNameOfTheSystem}/{uniqueShortenedValue}";
            var body = Encoding.UTF8.GetBytes(shortenedUrl);
            _channel.BasicPublish(exchange: "", routingKey: QueueName, body: body, basicProperties: null);
        }
    }

    private string GenerateRandomShortenedValue()
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
        int increasedAmount = (int)(_currentQueueSize + (_currentQueueSize * byPercentage / 100));

        _maxQueueSize = increasedAmount;
    }

    private void DeclareQueueAndUpdateCurrentQueueSize()
    {
        var queueDeclareOk = _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _currentQueueSize = queueDeclareOk.MessageCount;
    }

    private bool QueueAcceptsEntries()
    {
        DeclareQueueAndUpdateCurrentQueueSize();
        return _currentQueueSize <= _maxQueueSize;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}