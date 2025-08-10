using Domain.Aggregates.Camera;
using Domain.Aggregates.Camera.ValueObjects;
using DTOs.Camera;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CameraService.Services;

namespace CameraService.Services
{
    public class CameraService : ICameraService
    {
        private readonly Persistence.IUnitOfWork _unitOfWork;

        public CameraService(Persistence.IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CameraViewModel>> CreateCameraAsync(CreateCameraCommand command)
        {
            // Check if camera with same name already exists
            var existingCamera = await _unitOfWork.CameraRepository.ExistsWithNameAsync(command.Name);
            if (existingCamera)
            {
                return Result.Fail<CameraViewModel>("Camera with this name already exists");
            }

            // Check if camera with same network already exists
            var existingNetwork = await _unitOfWork.CameraRepository.ExistsWithNetworkAsync(command.IpAddress, command.Port);
            if (existingNetwork)
            {
                return Result.Fail<CameraViewModel>("Camera with this IP address and port already exists");
            }

            // Create camera based on type
            Result<Camera> cameraResult;
            
            switch (command.Type)
            {
                case CameraType.IP:
                    cameraResult = Camera.CreateIPCamera(
                        command.Name,
                        command.Location,
                        command.IpAddress,
                        command.Port,
                        command.Username,
                        command.Password
                    );
                    break;
                    
                case CameraType.RTSP:
                    var rtspUrl = $"rtsp://{command.IpAddress}:{command.Port}/stream";
                    cameraResult = Camera.CreateRTSPCamera(
                        command.Name,
                        command.Location,
                        rtspUrl,
                        command.Username,
                        command.Password
                    );
                    break;
                    
                case CameraType.ONVIF:
                    cameraResult = Camera.CreateONVIFCamera(
                        command.Name,
                        command.Location,
                        command.IpAddress,
                        command.Port,
                        command.Username,
                        command.Password
                    );
                    break;
                    
                default:
                    return Result.Fail<CameraViewModel>("Invalid camera type");
            }

            if (cameraResult.IsFailed)
            {
                return Result.Fail<CameraViewModel>(cameraResult.Errors);
            }

            // Save to repository
            await _unitOfWork.CameraRepository.AddAsync(cameraResult.Value);
            await _unitOfWork.SaveAsync();

            // Return ViewModel
            var cameraViewModel = new CameraViewModel
            {
                Id = cameraResult.Value.Id,
                Name = cameraResult.Value.Name,
                Location = cameraResult.Value.Location.Value,
                LocationZone = cameraResult.Value.Location.Zone,
                IpAddress = cameraResult.Value.Network.IpAddress,
                Port = cameraResult.Value.Network.Port,
                Type = cameraResult.Value.Type,
                Status = cameraResult.Value.Status,
                CreatedAt = cameraResult.Value.CreatedAt,
                LastActiveAt = cameraResult.Value.LastActiveAt,
                IsOnline = cameraResult.Value.IsOnline()
            };

            return Result.Ok(cameraViewModel);
        }

        public async Task<Result<CameraViewModel>> GetCameraByIdAsync(Guid id)
        {
            var camera = await _unitOfWork.CameraRepository.FindAsync(id);
            if (camera == null)
            {
                return Result.Fail<CameraViewModel>("Camera not found");
            }

            var cameraViewModel = new CameraViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                Location = camera.Location.Value,
                LocationZone = camera.Location.Zone,
                IpAddress = camera.Network.IpAddress,
                Port = camera.Network.Port,
                Type = camera.Type,
                Status = camera.Status,
                CreatedAt = camera.CreatedAt,
                LastActiveAt = camera.LastActiveAt,
                IsOnline = camera.IsOnline()
            };

            return Result.Ok(cameraViewModel);
        }

        public async Task<Result<IEnumerable<CameraViewModel>>> GetAllCamerasAsync()
        {
            var cameras = await _unitOfWork.CameraRepository.GetAllAsync();
            
            var cameraViewModels = cameras.Select(camera => new CameraViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                Location = camera.Location.Value,
                LocationZone = camera.Location.Zone,
                IpAddress = camera.Network.IpAddress,
                Port = camera.Network.Port,
                Type = camera.Type,
                Status = camera.Status,
                CreatedAt = camera.CreatedAt,
                LastActiveAt = camera.LastActiveAt,
                IsOnline = camera.IsOnline()
            });

            return Result.Ok(cameraViewModels);
        }

        public async Task<Result<IEnumerable<CameraSummaryViewModel>>> GetCameraSummariesAsync()
        {
            var cameras = await _unitOfWork.CameraRepository.GetAllAsync();
            
            var cameraSummaries = cameras.Select(camera => new CameraSummaryViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                Location = camera.Location.Value,
                Status = camera.Status,
                Type = camera.Type,
                IsOnline = camera.IsOnline()
            });

            return Result.Ok(cameraSummaries);
        }

        public async Task<Result<CameraViewModel>> UpdateCameraAsync(UpdateCameraCommand command)
        {
            var camera = await _unitOfWork.CameraRepository.FindAsync(command.Id);
            if (camera == null)
            {
                return Result.Fail<CameraViewModel>("Camera not found");
            }

            // Update name if provided
            if (!string.IsNullOrEmpty(command.Name) && command.Name != camera.Name)
            {
                var nameUpdateResult = camera.UpdateName(command.Name);
                if (nameUpdateResult.IsFailed)
                {
                    return Result.Fail<CameraViewModel>(nameUpdateResult.Errors);
                }
            }

            // Update location if provided
            if (!string.IsNullOrEmpty(command.Location))
            {
                var locationResult = CameraLocation.Create(command.Location, command.LocationZone, command.Latitude, command.Longitude);
                if (locationResult.IsFailed)
                {
                    return Result.Fail<CameraViewModel>(locationResult.Errors);
                }

                var locationUpdateResult = camera.UpdateLocation(locationResult.Value);
                if (locationUpdateResult.IsFailed)
                {
                    return Result.Fail<CameraViewModel>(locationUpdateResult.Errors);
                }
            }

            await _unitOfWork.CameraRepository.UpdateAsync(camera);
            await _unitOfWork.SaveAsync();

            var cameraViewModel = new CameraViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                Location = camera.Location.Value,
                LocationZone = camera.Location.Zone,
                IpAddress = camera.Network.IpAddress,
                Port = camera.Network.Port,
                Type = camera.Type,
                Status = camera.Status,
                CreatedAt = camera.CreatedAt,
                LastActiveAt = camera.LastActiveAt,
                IsOnline = camera.IsOnline()
            };

            return Result.Ok(cameraViewModel);
        }

        public async Task<Result> DeleteCameraAsync(Guid id)
        {
            var camera = await _unitOfWork.CameraRepository.FindAsync(id);
            if (camera == null)
            {
                return Result.Fail("Camera not found");
            }

            await _unitOfWork.CameraRepository.RemoveAsync(camera);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> ConnectCameraAsync(Guid id)
        {
            var camera = await _unitOfWork.CameraRepository.FindAsync(id);
            if (camera == null)
            {
                return Result.Fail("Camera not found");
            }

            try
            {
                camera.Connect();
                await _unitOfWork.CameraRepository.UpdateAsync(camera);
                await _unitOfWork.SaveAsync();
                
                return Result.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> DisconnectCameraAsync(Guid id)
        {
            var camera = await _unitOfWork.CameraRepository.FindAsync(id);
            if (camera == null)
            {
                return Result.Fail("Camera not found");
            }

            try
            {
                camera.Disconnect();
                await _unitOfWork.CameraRepository.UpdateAsync(camera);
                await _unitOfWork.SaveAsync();
                
                return Result.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result<IEnumerable<CameraViewModel>>> GetCamerasByStatusAsync(CameraStatus status)
        {
            var cameras = await _unitOfWork.CameraRepository.GetByStatusAsync(status);
            
            var cameraViewModels = cameras.Select(camera => new CameraViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                Location = camera.Location.Value,
                LocationZone = camera.Location.Zone,
                IpAddress = camera.Network.IpAddress,
                Port = camera.Network.Port,
                Type = camera.Type,
                Status = camera.Status,
                CreatedAt = camera.CreatedAt,
                LastActiveAt = camera.LastActiveAt,
                IsOnline = camera.IsOnline()
            });

            return Result.Ok(cameraViewModels);
        }

        public async Task<Result<IEnumerable<CameraViewModel>>> GetCamerasByLocationAsync(string location)
        {
            var cameras = await _unitOfWork.CameraRepository.GetByLocationAsync(location);
            
            var cameraViewModels = cameras.Select(camera => new CameraViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                Location = camera.Location.Value,
                LocationZone = camera.Location.Zone,
                IpAddress = camera.Network.IpAddress,
                Port = camera.Network.Port,
                Type = camera.Type,
                Status = camera.Status,
                CreatedAt = camera.CreatedAt,
                LastActiveAt = camera.LastActiveAt,
                IsOnline = camera.IsOnline()
            });

            return Result.Ok(cameraViewModels);
        }

        public async Task<Result> UpdateCameraHeartbeatAsync(Guid id)
        {
            var camera = await _unitOfWork.CameraRepository.FindAsync(id);
            if (camera == null)
            {
                return Result.Fail("Camera not found");
            }

            camera.UpdateHeartbeat();
            await _unitOfWork.CameraRepository.UpdateAsync(camera);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result<CameraConfigurationViewModel>> GetCameraConfigurationAsync(Guid id)
        {
            var camera = await _unitOfWork.CameraRepository.FindAsync(id);
            if (camera == null)
            {
                return Result.Fail<CameraConfigurationViewModel>("Camera not found");
            }

            if (camera.Configuration == null)
            {
                return Result.Fail<CameraConfigurationViewModel>("Camera configuration not found");
            }

            var configurationViewModel = new CameraConfigurationViewModel
            {
                Id = camera.Configuration.Id,
                Resolution = camera.Configuration.Resolution,
                FrameRate = camera.Configuration.FrameRate,
                VideoCodec = camera.Configuration.VideoCodec,
                Bitrate = camera.Configuration.Bitrate,
                AudioEnabled = camera.Configuration.AudioEnabled,
                AudioCodec = camera.Configuration.AudioCodec,
                MotionDetectionEnabled = camera.Configuration.MotionDetection.Enabled,
                MotionSensitivity = camera.Configuration.MotionDetection.Sensitivity,
                RecordingEnabled = camera.Configuration.Recording.Enabled
            };

            return Result.Ok(configurationViewModel);
        }

        public async Task<Result> UpdateCameraConfigurationAsync(Guid id, CameraConfigurationViewModel configuration)
        {
            var camera = await _unitOfWork.CameraRepository.FindAsync(id);
            if (camera == null)
            {
                return Result.Fail("Camera not found");
            }

            try
            {
                camera.UpdateVideoSettings(
                    configuration.Resolution,
                    configuration.FrameRate,
                    configuration.VideoCodec,
                    configuration.Bitrate
                );

                camera.UpdateAudioSettings(
                    configuration.AudioEnabled,
                    configuration.AudioCodec
                );

                await _unitOfWork.CameraRepository.UpdateAsync(camera);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to update camera configuration: {ex.Message}");
            }
        }
    }
}
