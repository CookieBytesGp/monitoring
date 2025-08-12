
using Microsoft.Extensions.Logging;
using Monitoring.Application.Interfaces.Camera;
using Monitoring.Application.Interfaces.Realtime;
using Monitoring.Common.Interfaces;
using Monitoring.Common.Utilities;
using Monitoring.Application.DTOs;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.Services.Camera;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monitoring.Infrastructure.Persistence;
using FluentResults;
using Monitoring.Application.DTOs.Camera;
using AutoMapper;
using System;
using System;
using System.Linq;
using Domain.Aggregates.Camera.ValueObjects;

namespace Monitoring.Application.Services.Camera
{
    public class CameraService : ICameraService
    {
        private readonly Monitoring.Infrastructure.Persistence.IUnitOfWork _unitOfWork;
        private readonly ICameraNotifications _cameraNotifications;
        private readonly ILogger<CameraService> _logger;
        private readonly ICameraStreamService _cameraStreamService;
        private readonly IMapper _mapper;
        private readonly Monitoring.Domain.Services.Camera.ICameraStrategyFactory _strategyFactory;

        public CameraService(
            ICameraNotifications cameraNotifications,
            Monitoring.Infrastructure.Persistence.IUnitOfWork unitOfWork,
            ILogger<CameraService> logger,
            IMapper mapper,
            ICameraStreamService cameraStreamService,
            Monitoring.Domain.Services.Camera.ICameraStrategyFactory strategyFactory)
        {
            _cameraNotifications = cameraNotifications;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _cameraStreamService = cameraStreamService;
            _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        }
        private Monitoring.Domain.Aggregates.Camera.Camera CreateCameraFromDto(CameraDto dto)
        {
            if (dto == null) return null;

            var type = CameraType.GetAll<CameraType>()
                .FirstOrDefault(t => t.Name.Equals(dto.Type, StringComparison.OrdinalIgnoreCase) || t.Value.ToString() == dto.Type)
                ?? CameraType.IP;

            var result = Monitoring.Domain.Aggregates.Camera.Camera.Create(
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

            // Set Id if provided (for update scenarios)
            if (dto.Id != Guid.Empty)
            {
                camera.GetType().GetProperty("Id")?.SetValue(camera, dto.Id);
            }

            return camera;
        }

        public async Task<Result<List<CameraDto>>> GetAllCamerasAsync()
        {
            try
            {
                var cameras = await _unitOfWork.CameraRepository.GetAllAsync();
                var dtos = _mapper.Map<List<CameraDto>>(cameras);
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
                var dto = _mapper.Map<CameraDto>(camera);
                return Result.Ok(dto);
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
                var entity = _mapper.Map<Domain.Aggregates.Camera.Camera>(cameraDto);
                if (entity == null)
                    return Result.Fail<CameraDto>("Invalid camera data");
                await _unitOfWork.CameraRepository.AddAsync(entity);
                await _unitOfWork.SaveAsync();
                var resultDto = _mapper.Map<CameraDto>(entity);
                return Result.Ok(resultDto);
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
                var entity = CreateCameraFromDto(cameraDto);
                if (entity == null)
                    return Result.Fail<CameraDto>("Invalid camera data");
                await _unitOfWork.CameraRepository.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();
                var resultDto = _mapper.Map<CameraDto>(entity);
                return Result.Ok(resultDto);
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
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");

                var streamResult = await _cameraStreamService.GetStreamUrlAsync(id);
                if (streamResult.IsFailed)
                    return streamResult;

                _logger.LogInformation($"Stream URL generated for camera {id}: {streamResult.Value}");
                return streamResult;
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

                var connectionResult = await _cameraStreamService.TestConnectionAsync(id);
                if (connectionResult.IsFailed)
                    return connectionResult;

                bool isConnected = connectionResult.Value;
                await _cameraNotifications.UpdateCameraStatusAsync(id, isConnected);
                
                _logger.LogInformation($"Camera {id} connection test result: {isConnected}");
                return Result.Ok(isConnected);
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
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");

                var snapshotResult = await _cameraStreamService.GetSnapshotUrlAsync(id);
                if (snapshotResult.IsFailed)
                    return snapshotResult;

                _logger.LogInformation($"Snapshot URL generated for camera {id}: {snapshotResult.Value}");
                return snapshotResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCameraSnapshotAsync");
                return Result.Fail<string>(ex.Message);
            }
        }

        public async Task<Result<byte[]>> CaptureSnapshotAsync(Guid id)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<byte[]>("Camera not found");

                var snapshotResult = await _cameraStreamService.CaptureSnapshotAsync(id);
                if (snapshotResult.IsFailed)
                    return snapshotResult;

                _logger.LogInformation($"Snapshot captured for camera {id}, size: {snapshotResult.Value.Length} bytes");
                return snapshotResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CaptureSnapshotAsync");
                return Result.Fail<byte[]>(ex.Message);
            }
        }

        public async Task<Result<string>> GetRtspStreamUrlAsync(Guid id)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");

                var rtspResult = await _cameraStreamService.GetRtspStreamUrlAsync(id);
                if (rtspResult.IsFailed)
                    return rtspResult;

                _logger.LogInformation($"RTSP URL generated for camera {id}: {rtspResult.Value}");
                return rtspResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRtspStreamUrlAsync");
                return Result.Fail<string>(ex.Message);
            }
        }

        public async Task<Result<string>> GetHttpStreamUrlAsync(Guid id)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(id);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");

                var httpResult = await _cameraStreamService.GetHttpStreamUrlAsync(id);
                if (httpResult.IsFailed)
                    return httpResult;

                _logger.LogInformation($"HTTP stream URL generated for camera {id}: {httpResult.Value}");
                return httpResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetHttpStreamUrlAsync");
                return Result.Fail<string>(ex.Message);
            }
        }

        #region Factory Pattern Methods

        /// <summary>
        /// اتصال به دوربین با استفاده از Factory Pattern (انتخاب خودکار استراتژی)
        /// </summary>
        public async Task<Result<CameraConnectionInfo>> ConnectWithBestStrategyAsync(Guid cameraId)
        {
            try
            {
                _logger.LogInformation("Connecting to camera {CameraId} using best strategy", cameraId);

                var camera = await _unitOfWork.CameraRepository.GetByIdAsync(cameraId);
                if (camera == null)
                    return Result.Fail<CameraConnectionInfo>("Camera not found");

                // دریافت بهترین استراتژی
                var bestStrategyResult = await _strategyFactory.GetBestStrategyAsync(camera);
                if (bestStrategyResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(bestStrategyResult.Errors);

                var strategy = bestStrategyResult.Value;
                
                // اتصال با استراتژی انتخاب شده
                var connectionResult = await strategy.ConnectAsync(camera);
                if (connectionResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(connectionResult.Errors);

                // آپدیت اطلاعات اتصال در دوربین
                camera.SetConnectionInfo(connectionResult.Value);
                await _unitOfWork.CameraRepository.UpdateAsync(camera);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Successfully connected to camera {CameraId} using strategy {StrategyName}", 
                    cameraId, strategy.StrategyName);

                return Result.Ok(connectionResult.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to camera {CameraId} with best strategy", cameraId);
                return Result.Fail<CameraConnectionInfo>($"Connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// دریافت تمام استراتژی‌های پشتیبان شده برای دوربین
        /// </summary>
        public async Task<Result<List<string>>> GetSupportedStrategiesAsync(Guid cameraId)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.GetByIdAsync(cameraId);
                if (camera == null)
                    return Result.Fail<List<string>>("Camera not found");

                var strategiesResult = await _strategyFactory.GetSupportedStrategiesAsync(camera);
                if (strategiesResult.IsFailed)
                    return Result.Fail<List<string>>(strategiesResult.Errors);

                var strategyNames = strategiesResult.Value.Select(s => s.StrategyName).ToList();
                return Result.Ok(strategyNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported strategies for camera {CameraId}", cameraId);
                return Result.Fail<List<string>>($"Failed to get strategies: {ex.Message}");
            }
        }

        /// <summary>
        /// تست تمام استراتژی‌ها برای دوربین
        /// </summary>
        public async Task<Result<Dictionary<string, bool>>> TestAllStrategiesAsync(Guid cameraId)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.GetByIdAsync(cameraId);
                if (camera == null)
                    return Result.Fail<Dictionary<string, bool>>("Camera not found");

                var results = new Dictionary<string, bool>();
                var allStrategies = _strategyFactory.GetAllStrategies();

                foreach (var strategy in allStrategies)
                {
                    try
                    {
                        if (strategy.SupportsCamera(camera))
                        {
                            var testResult = await strategy.TestConnectionAsync(camera);
                            results[strategy.StrategyName] = testResult.IsSuccess && testResult.Value;
                        }
                        else
                        {
                            results[strategy.StrategyName] = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error testing strategy {StrategyName} for camera {CameraId}", 
                            strategy.StrategyName, cameraId);
                        results[strategy.StrategyName] = false;
                    }
                }

                return Result.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing strategies for camera {CameraId}", cameraId);
                return Result.Fail<Dictionary<string, bool>>($"Strategy testing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// اتصال با استراتژی مشخص
        /// </summary>
        public async Task<Result<CameraConnectionInfo>> ConnectWithStrategyAsync(Guid cameraId, string strategyName)
        {
            try
            {
                _logger.LogInformation("Connecting to camera {CameraId} using strategy {StrategyName}", 
                    cameraId, strategyName);

                var camera = await _unitOfWork.CameraRepository.GetByIdAsync(cameraId);
                if (camera == null)
                    return Result.Fail<CameraConnectionInfo>("Camera not found");

                var strategyResult = _strategyFactory.GetStrategyByName(strategyName);
                if (strategyResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(strategyResult.Errors);

                var strategy = strategyResult.Value;

                if (!strategy.SupportsCamera(camera))
                    return Result.Fail<CameraConnectionInfo>($"Strategy {strategyName} does not support this camera");

                var connectionResult = await strategy.ConnectAsync(camera);
                if (connectionResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(connectionResult.Errors);

                camera.SetConnectionInfo(connectionResult.Value);
                await _unitOfWork.CameraRepository.UpdateAsync(camera);
                var saveResult = await _unitOfWork.SaveAsync();

                return Result.Ok(connectionResult.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to camera {CameraId} with strategy {StrategyName}", 
                    cameraId, strategyName);
                return Result.Fail<CameraConnectionInfo>($"Connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// گرفتن اسکرین‌شات با بهترین استراتژی
        /// </summary>
        public async Task<Result<byte[]>> CaptureSnapshotWithBestStrategyAsync(Guid cameraId)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.GetByIdAsync(cameraId);
                if (camera == null)
                    return Result.Fail<byte[]>("Camera not found");

                var bestStrategyResult = await _strategyFactory.GetBestStrategyAsync(camera);
                if (bestStrategyResult.IsFailed)
                    return Result.Fail<byte[]>(bestStrategyResult.Errors);

                var strategy = bestStrategyResult.Value;
                var snapshotResult = await strategy.CaptureSnapshotAsync(camera);

                if (snapshotResult.IsSuccess)
                {
                    _logger.LogInformation("Snapshot captured from camera {CameraId} using strategy {StrategyName}", 
                        cameraId, strategy.StrategyName);
                }

                return snapshotResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing snapshot from camera {CameraId}", cameraId);
                return Result.Fail<byte[]>($"Snapshot capture failed: {ex.Message}");
            }
        }

        #endregion
    }
}