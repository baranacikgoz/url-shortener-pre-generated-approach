using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using UrlShortener.Database;

namespace UrlShortener.Services;

public class ShortenedUrlsBackgroundService : IHostedService
{
    private readonly string _domainNameOfTheSystem;
    private readonly int _shortenedValueLength;
    private readonly int _queueSize;
    private readonly int _waitForMsIfQueueFull;
    private readonly IUrlRepository _urlRepository;
    private readonly ConcurrentQueue<string> _shortenedUrls;
    private readonly Random _random = new();
    private const string _alphabet = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public ShortenedUrlsBackgroundService(IConfiguration configuration, IUrlRepository urlRepository, ConcurrentQueue<string> shortenedValues)
    {
        _domainNameOfTheSystem = configuration.GetValue<string>("DomainNameOfTheSystem") ?? throw new ApplicationException("DomainNameOfTheSystem is not found!");
        _shortenedValueLength = configuration.GetValue<int>("ShortenedLength");
        _queueSize = configuration.GetValue<int>("QueueSize");
        _waitForMsIfQueueFull = configuration.GetValue<int>("WaitForMsIfQueueFull");
        _urlRepository = urlRepository;
        _shortenedUrls = shortenedValues;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_shortenedUrls.Count > _queueSize)
            {
                await Task.Delay(_waitForMsIfQueueFull, cancellationToken);
                continue;
            }

            string uniqueShortenedValue;
            do
            {
                uniqueShortenedValue = GenerateRandomShortenedValue();
            }
            while (await _urlRepository.ShortenedValueExists(uniqueShortenedValue, cancellationToken) && !cancellationToken.IsCancellationRequested);

            string shortenedUrl = $"https://{_domainNameOfTheSystem}/{uniqueShortenedValue}";
            _shortenedUrls.Enqueue(shortenedUrl);
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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}