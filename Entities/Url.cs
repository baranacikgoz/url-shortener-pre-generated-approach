namespace UrlShortener.Entities;

public class Url
{
    public DefaultIdType Id { get; set; }
    public string OriginalUrl { get; set; } = default!;
    public string ShortenedUrl { get; set; } = default!;
}