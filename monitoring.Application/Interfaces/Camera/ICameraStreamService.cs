using FluentResults;
using System;
using System.Threading.Tasks;

namespace Monitoring.Application.Interfaces.Camera
{
    public interface ICameraStreamService
    {
        Task<Result<string>> GetStreamUrlAsync(Guid cameraId);
        Task<Result<string>> GetSnapshotUrlAsync(Guid cameraId);
        Task<Result<bool>> TestConnectionAsync(Guid cameraId);
        Task<Result<byte[]>> CaptureSnapshotAsync(Guid cameraId);
        Task<Result<string>> GetRtspStreamUrlAsync(Guid cameraId);
        Task<Result<string>> GetHttpStreamUrlAsync(Guid cameraId);
    }
}
