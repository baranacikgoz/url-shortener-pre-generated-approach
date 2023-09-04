using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace UrlShortener.RetryPolicies;

public static class PollyPolicies
{
    public static AsyncRetryPolicy<BasicGetResult?> QueueRetrievalRetryPolicy(Action<DelegateResult<BasicGetResult?>, TimeSpan, int, Context> onRetry)
    {
        return Policy
            .HandleResult<BasicGetResult?>(r => r is null)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 300),
                onRetry: onRetry);
    }

    public static AsyncRetryPolicy<string?> CacheGetRetryPolicy(Action<DelegateResult<string?>, TimeSpan, int, Context> onRetry)
    {
        return Policy
            .Handle<Exception>()
            .OrResult<string?>(r => r is null)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 100),
                onRetry: onRetry);
    }

    public static AsyncRetryPolicy CacheSetRetryPolicy(Action<Exception, TimeSpan, int, Context> onRetry)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 100),
                onRetry: onRetry);
    }
}