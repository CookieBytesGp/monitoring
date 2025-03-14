 using App.Models.Camera;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Interfaces
{
    public interface ICameraService
    {
        Task<IEnumerable<CameraDevice>> GetAllCamerasAsync();
        Task<CameraDevice> GetCameraByIdAsync(int id);
        Task<CameraDevice> CreateCameraAsync(CameraDevice camera);
        Task<CameraDevice> UpdateCameraAsync(CameraDevice camera);
        Task<bool> DeleteCameraAsync(int id);
        Task<bool> UpdateCameraStatusAsync(int id, bool isActive);
        Task<Stream> GetCameraStreamAsync(int id);
        Task<bool> TestCameraConnectionAsync(int id);
        Task<string> GetCameraSnapshotAsync(int id);
    }
}