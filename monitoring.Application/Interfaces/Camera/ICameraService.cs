
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Monitoring.Application.DTOs.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;

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
    
    // متدهای اضافی برای کار با stream و تصاویر
    Task<Result<byte[]>> CaptureSnapshotAsync(Guid id);
    Task<Result<string>> GetRtspStreamUrlAsync(Guid id);
    Task<Result<string>> GetHttpStreamUrlAsync(Guid id);

    // متدهای Factory Pattern
    Task<Result<CameraConnectionInfo>> ConnectWithBestStrategyAsync(Guid cameraId);
    Task<Result<List<string>>> GetSupportedStrategiesAsync(Guid cameraId);
    Task<Result<Dictionary<string, bool>>> TestAllStrategiesAsync(Guid cameraId);
    Task<Result<CameraConnectionInfo>> ConnectWithStrategyAsync(Guid cameraId, string strategyName);
    Task<Result<byte[]>> CaptureSnapshotWithBestStrategyAsync(Guid cameraId);
}
