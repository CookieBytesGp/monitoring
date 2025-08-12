
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Monitoring.Application.DTOs.Camera;

namespace Monitoring.Application.Interfaces.Camera;

public interface ICameraService
{
    Task<Result<List<CameraDto>>> GetAllCamerasAsync();
    Task<Result<CameraDto>> GetCameraByIdAsync(Guid id);
    Task<Result<CameraDto>> CreateCameraAsync(CameraDto cameraDto);
    Task<Result<CameraDto>> UpdateCameraAsync(CameraDto cameraDto);
    Task<Result<bool>> DeleteCameraAsync(Guid id);
    Task<Result<bool>> UpdateCameraStatusAsync(Guid id, bool isActive);
    Task<Result<string>> GetCameraStreamAsync(Guid id);
    Task<Result<bool>> TestCameraConnectionAsync(Guid id);
    Task<Result<string>> GetCameraSnapshotAsync(Guid id);
}
