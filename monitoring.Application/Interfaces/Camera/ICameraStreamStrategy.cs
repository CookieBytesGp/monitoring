using System.Threading.Tasks;
using Domain.Aggregates.Camera.ValueObjects;

namespace Monitoring.Application.Interfaces.Camera;

public interface ICameraStreamStrategy
{
    Task<string> GetStreamUrlAsync(Domain.Aggregates.Camera.Camera camera, StreamQuality quality = null);
    Task<byte[]> CaptureSnapshotAsync(Domain.Aggregates.Camera.Camera camera);
    Task<bool> TestConnectionAsync(Domain.Aggregates.Camera.Camera camera);
    bool SupportsCamera(Domain.Aggregates.Camera.Camera camera);
}

