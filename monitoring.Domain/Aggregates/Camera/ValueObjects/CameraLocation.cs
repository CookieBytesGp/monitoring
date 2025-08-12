using FluentResults;
using Monitoring.Domain.SeedWork;

namespace Domain.Aggregates.Camera.ValueObjects;

public class CameraLocation : ValueObject
{
    public string Value { get; private set; }
    public string Zone { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    private CameraLocation()
    {
        
    }

    private CameraLocation(string value, string zone = null, double? latitude = null, double? longitude = null)
    {
        Value = value.Trim();
        Zone = zone?.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Result<CameraLocation> Create(string value, string zone = null, double? latitude = null, double? longitude = null)
    {
        var result = new Result<CameraLocation>();

        if (string.IsNullOrWhiteSpace(value))
        {
            result.WithError("Camera location cannot be empty");
            return result;
        }

        if (value.Length > 200)
        {
            result.WithError("Camera location cannot exceed 200 characters");
            return result;
        }

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
        {
            result.WithError("Latitude must be between -90 and 90");
            return result;
        }

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
        {
            result.WithError("Longitude must be between -180 and 180");
            return result;
        }

        var cameraLocation = new CameraLocation(value, zone, latitude, longitude);
        result.WithValue(cameraLocation);
        return result;
    }

    public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;

    public bool HasZone => !string.IsNullOrWhiteSpace(Zone);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
        yield return Zone?.ToUpperInvariant();
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString()
    {
        var location = Value;
        if (HasZone)
            location += $" ({Zone})";
        
        if (HasCoordinates)
            location += $" [{Latitude:F6}, {Longitude:F6}]";

        return location;
    }

    public static implicit operator string(CameraLocation location)
    {
        return location.Value;
    }
}
