using DTOs.Camera;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CameraService.Services
{
    public interface ICameraService
    {
        Task<Result<CameraViewModel>> CreateCameraAsync(CreateCameraCommand command);
        Task<Result<CameraViewModel>> GetCameraByIdAsync(Guid id);
        Task<Result<IEnumerable<CameraViewModel>>> GetAllCamerasAsync();
        Task<Result<IEnumerable<CameraSummaryViewModel>>> GetCameraSummariesAsync();
        Task<Result<CameraViewModel>> UpdateCameraAsync(UpdateCameraCommand command);
        Task<Result> DeleteCameraAsync(Guid id);
        Task<Result> ConnectCameraAsync(Guid id);
        Task<Result> DisconnectCameraAsync(Guid id);
        Task<Result<IEnumerable<CameraViewModel>>> GetCamerasByStatusAsync(Domain.Aggregates.Camera.CameraStatus status);
        Task<Result<IEnumerable<CameraViewModel>>> GetCamerasByLocationAsync(string location);
        Task<Result> UpdateCameraHeartbeatAsync(Guid id);
        Task<Result<CameraConfigurationViewModel>> GetCameraConfigurationAsync(Guid id);
        Task<Result> UpdateCameraConfigurationAsync(Guid id, CameraConfigurationViewModel configuration);
    }
}
