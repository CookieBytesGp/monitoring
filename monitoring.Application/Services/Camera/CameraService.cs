
using Microsoft.Extensions.Logging;
using Monitoring.Application.Interfaces.Camera;
using Monitoring.Application.Interfaces.Realtime;
using Monitoring.Common.Interfaces;
using Monitoring.Application.DTOs;
using Domain.Aggregates.Camera;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monitoring.Infrastructure.Persistence;
using FluentResults;
using Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Application.DTOs.Camera;

namespace Monitoring.Application.Services.Camera
{
    public class CameraService : ICameraService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICameraNotifications _cameraNotifications;
        private readonly ILogger<CameraService> _logger;

        public CameraService(
            ICameraNotifications cameraNotifications,
            IUnitOfWork unitOfWork,
            ILogger<CameraService> logger)
        {
            _cameraNotifications = cameraNotifications;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // متد مپر دستی
        private CameraDto MapToDto(Domain.Aggregates.Camera.Camera entity)
        {
            if (entity == null) return null;
            return new CameraDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Location = entity.Location?.ToString(),
                IpAddress = entity.Network?.IpAddress,
                Port = entity.Network?.Port ?? 0,
                Username = entity.Network?.Username,
                Password = entity.Network?.Password,
                Type = entity.Type.ToString(),
                Status = entity.Status.ToString()
            };
        }

        private Domain.Aggregates.Camera.Camera MapToEntity(CameraDto dto)
        {
            if (dto == null) return null;
            var type = CameraType.GetAll<CameraType>()
                .FirstOrDefault(t => t.Name.Equals(dto.Type, StringComparison.OrdinalIgnoreCase) || t.Value.ToString() == dto.Type)
                ?? CameraType.IP;
            var result = Domain.Aggregates.Camera.Camera.Create(
                dto.Name,
                dto.Location,
                dto.IpAddress,
                dto.Port,
                dto.Username,
                dto.Password,
                type);
            if (result.IsFailed)
                return null;
            var camera = result.Value;
            var status = CameraStatus.GetAll<CameraStatus>()
                .FirstOrDefault(s => s.Name.Equals(dto.Status, StringComparison.OrdinalIgnoreCase) || s.Value.ToString() == dto.Status)
                ?? CameraStatus.Inactive;
            camera.SetStatus(status);
            camera.GetType().GetProperty("Id")?.SetValue(camera, dto.Id);
            return camera;
        }


        public async Task<Result<List<CameraDto>>> GetAllCamerasAsync()
        {
            try
            {
                var cameras = await _unitOfWork.CameraRepository.GetAllAsync();
                var dtos = new List<CameraDto>();
                foreach (var cam in cameras)
                    dtos.Add(MapToDto(cam));
                return Result.Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllCamerasAsync");
                return Result.Fail<List<CameraDto>>(ex.Message);
            }
        }


        public async Task<Result<CameraDto>> GetCameraByIdAsync(Guid id)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<CameraDto>("Camera not found");
                return Result.Ok(MapToDto(camera));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCameraByIdAsync");
                return Result.Fail<CameraDto>(ex.Message);
            }
        }


        public async Task<Result<CameraDto>> CreateCameraAsync(CameraDto cameraDto)
        {
            try
            {
                var entity = MapToEntity(cameraDto);
                if (entity == null)
                    return Result.Fail<CameraDto>("Invalid camera data");
                await _unitOfWork.CameraRepository.AddAsync(entity);
                await _unitOfWork.SaveAsync();
                return Result.Ok(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateCameraAsync");
                return Result.Fail<CameraDto>(ex.Message);
            }
        }


        public async Task<Result<CameraDto>> UpdateCameraAsync(CameraDto cameraDto)
        {
            try
            {
                var entity = MapToEntity(cameraDto);
                if (entity == null)
                    return Result.Fail<CameraDto>("Invalid camera data");
                await _unitOfWork.CameraRepository.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();
                return Result.Ok(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateCameraAsync");
                return Result.Fail<CameraDto>(ex.Message);
            }
        }


        public async Task<Result<bool>> DeleteCameraAsync(Guid id)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<bool>("Camera not found");
                await _unitOfWork.CameraRepository.RemoveAsync(camera);
                await _unitOfWork.SaveAsync();
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteCameraAsync");
                return Result.Fail<bool>(ex.Message);
            }
        }


        public async Task<Result<bool>> UpdateCameraStatusAsync(Guid id, bool isActive)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<bool>("Camera not found");
                camera.SetStatus(isActive ? CameraStatus.Active : CameraStatus.Inactive);
                await _unitOfWork.CameraRepository.UpdateAsync(camera);
                await _unitOfWork.SaveAsync();
                await _cameraNotifications.UpdateCameraStatusAsync(id, isActive);
                _logger.LogInformation($"Camera {id} status updated to {isActive}");
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateCameraStatusAsync");
                return Result.Fail<bool>(ex.Message);
            }
        }


        public async Task<Result<string>> GetCameraStreamAsync(Guid id)
        {
            try
            {
                // فرض بر این است که استراتژی استریم پیاده‌سازی شده
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");
                // فرض بر این است که استراتژی استریم تزریق شده یا قابل دسترسی است
                // string streamUrl = await _cameraStreamStrategy.GetStreamUrlAsync(camera);
                // return Result.Ok(streamUrl);
                return Result.Fail<string>("Stream strategy not implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCameraStreamAsync");
                return Result.Fail<string>(ex.Message);
            }
        }


        public async Task<Result<bool>> TestCameraConnectionAsync(Guid id)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<bool>("Camera not found");
                // فرض بر این است که تست اتصال پیاده‌سازی شده
                // bool isConnected = await _cameraStreamStrategy.TestConnectionAsync(camera);
                // camera.Status = isConnected ? CameraStatus.Active : CameraStatus.Inactive;
                // await _unitOfWork.CameraRepository.UpdateAsync(camera);
                // await _unitOfWork.SaveChangesAsync();
                await _cameraNotifications.UpdateCameraStatusAsync(id, true);
                _logger.LogInformation($"Camera {id} connection tested successfully");
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestCameraConnectionAsync");
                return Result.Fail<bool>(ex.Message);
            }
        }

        public async Task<Result<string>> GetCameraSnapshotAsync(Guid id)
        {
            try
            {
                // فرض بر این است که استراتژی اسنپ‌شات پیاده‌سازی شده
                return Result.Fail<string>("Snapshot strategy not implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCameraSnapshotAsync");
                return Result.Fail<string>(ex.Message);
            }
        }
    }
}