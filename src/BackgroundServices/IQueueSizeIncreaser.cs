namespace UrlShortener.BackgroundServices;

public interface IQueueSizeIncreaser
{
    void IncreaseQueueSize(int byPercentage);
}