using System.Threading.Tasks;
using Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;

namespace Monitoring.Application.Interfaces.Camera;

public interface ICameraStreamStrategy
{
    Task<string> GetStreamUrlAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, StreamQuality quality = null);
    Task<byte[]> CaptureSnapshotAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);
    Task<bool> TestConnectionAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);
    bool SupportsCamera(Monitoring.Domain.Aggregates.Camera.Camera camera);
}

