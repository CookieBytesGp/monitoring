using Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.SeedWork;

namespace Domain.Aggregates.Camera.Entities;

public class CameraStream : Entity
{
    private CameraStream() : base()
    {
        
    }

    private CameraStream(StreamQuality quality, string url, bool isActive = true)
    {
        Quality = quality;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        ValidateUrl();
    }

    public StreamQuality Quality { get; private set; }
    public string Url { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static CameraStream Create(StreamQuality quality, string url, bool isActive = true)
    {
        return new CameraStream(quality, url, isActive);
    }

    public void UpdateUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("Stream URL cannot be empty", nameof(newUrl));

        Url = newUrl;
        UpdatedAt = DateTime.UtcNow;
        ValidateUrl();
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateUrl()
    {
        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid stream URL format", nameof(Url));
    
        var supportedSchemes = new[] { "http", "https", "rtsp", "rtmp" };
        if (!supportedSchemes.Contains(uri.Scheme.ToLower()))
            throw new ArgumentException($"Unsupported URL scheme: {uri.Scheme}. Supported schemes: {string.Join(", ", supportedSchemes)}");
    }

    public string GetQualityDisplayName()
    {
    if (Quality == StreamQuality.Low) return "480p";
    if (Quality == StreamQuality.Medium) return "720p";
    if (Quality == StreamQuality.High) return "1080p";
    if (Quality == StreamQuality.Ultra) return "4K";
    return Quality.ToString();
    }
}
