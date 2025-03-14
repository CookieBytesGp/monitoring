using App.Models.Camera;
using App.Services.Interfaces;
using OpenCvSharp;
using System.Collections.Concurrent;

namespace App.Services
{
    public class MotionDetectionService : IMotionDetectionService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;
        private readonly IEmailService _emailService;
        private readonly ICameraService _cameraService;
        private readonly ConcurrentDictionary<string, MotionDetectionSettings> _settings;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _detectionTasks;
        private readonly string _baseImagePath;

        public MotionDetectionService(
            IConfiguration configuration,
            ILoggingService loggingService,
            IEmailService emailService,
            ICameraService cameraService)
        {
            _configuration = configuration;
            _loggingService = loggingService;
            _emailService = emailService;
            _cameraService = cameraService;
            _settings = new ConcurrentDictionary<string, MotionDetectionSettings>();
            _detectionTasks = new ConcurrentDictionary<string, CancellationTokenSource>();
            _baseImagePath = _configuration["MotionDetection:ImageSavePath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MotionDetection");
        }

        public async Task StartDetectionAsync(string cameraId)
        {
            if (_detectionTasks.ContainsKey(cameraId))
            {
                return;
            }

            var settings = await GetSettingsAsync(cameraId);
            var cts = new CancellationTokenSource();
            _detectionTasks[cameraId] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessCameraFeedAsync(cameraId, settings, cts.Token);
                }
                catch (Exception ex)
                {
                    await _loggingService.LogErrorAsync(ex, "MotionDetection", $"Error processing camera {cameraId}");
                }
            }, cts.Token);

            await _loggingService.LogSystemEventAsync("MotionDetection", $"Started motion detection for camera {cameraId}");
        }

        public async Task StopDetectionAsync(string cameraId)
        {
            if (_detectionTasks.TryRemove(cameraId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                await _loggingService.LogSystemEventAsync("MotionDetection", $"Stopped motion detection for camera {cameraId}");
            }
        }

        public Task<bool> IsDetectionActiveAsync(string cameraId)
        {
            return Task.FromResult(_detectionTasks.ContainsKey(cameraId));
        }

        public async Task UpdateSensitivityAsync(string cameraId, double sensitivity)
        {
            var settings = await GetSettingsAsync(cameraId);
            settings.Sensitivity = Math.Clamp(sensitivity, 0.0, 1.0);
            _settings[cameraId] = settings;
        }

        public async Task UpdateRegionOfInterestAsync(string cameraId, Rectangle roi)
        {
            var settings = await GetSettingsAsync(cameraId);
            settings.RegionOfInterest = roi;
            _settings[cameraId] = settings;
        }

        public async Task<MotionDetectionSettings> GetSettingsAsync(string cameraId)
        {
            if (_settings.TryGetValue(cameraId, out var existingSettings))
            {
                return existingSettings;
            }

            var settings = new MotionDetectionSettings
            {
                IsActive = true,
                Sensitivity = _configuration.GetValue<double>("MotionDetection:DefaultSensitivity", 0.3),
                RegionOfInterest = new Rectangle { X = 0, Y = 0, Width = 0, Height = 0 }, // Full frame
                MinimumMotionFrames = _configuration.GetValue<int>("MotionDetection:MinimumMotionFrames", 3),
                ThresholdValue = _configuration.GetValue<double>("MotionDetection:ThresholdValue", 25),
                SaveDetectionImages = _configuration.GetValue<bool>("MotionDetection:SaveImages", true),
                SavePath = Path.Combine(_baseImagePath, cameraId)
            };

            _settings[cameraId] = settings;
            return settings;
        }

        private async Task ProcessCameraFeedAsync(string cameraId, MotionDetectionSettings settings, CancellationToken ct)
        {
            if (!int.TryParse(cameraId, out int cameraIdInt))
            {
                throw new ArgumentException("Invalid camera ID format", nameof(cameraId));
            }
            var camera = await _cameraService.GetCameraByIdAsync(cameraIdInt);
            if (camera == null)
            {
                throw new ArgumentException("Camera not found", nameof(cameraId));
            }

            using var capture = new OpenCvSharp.VideoCapture(camera.StreamUrl); // Ensure this line is correct
            if (!capture.IsOpened()) return;

            using var prevFrame = new Mat();
            using var currentFrame = new Mat();
            using var diffFrame = new Mat();
            using var grayDiff = new Mat();
            using var blurredDiff = new Mat();
            using var thresholdFrame = new Mat();

            int motionFrameCount = 0;
            DateTime lastNotification = DateTime.MinValue;

            while (!ct.IsCancellationRequested)
            {
                if (!capture.Read(currentFrame) || currentFrame.Empty())
                {
                    await Task.Delay(100, ct);
                    continue;
                }

                if (prevFrame.Empty())
                {
                    currentFrame.CopyTo(prevFrame);
                    continue;
                }

                // Apply ROI if specified
                var roi = settings.RegionOfInterest;
                if (roi.Width > 0 && roi.Height > 0)
                {
                    using var roiFrame = new Mat(currentFrame, new Rect(roi.X, roi.Y, roi.Width, roi.Height));
                    using var roiPrevFrame = new Mat(prevFrame, new Rect(roi.X, roi.Y, roi.Width, roi.Height));
                    roiFrame.CopyTo(currentFrame);
                    roiPrevFrame.CopyTo(prevFrame);
                }

                // Calculate difference between frames
                Cv2.Absdiff(currentFrame, prevFrame, diffFrame);
                Cv2.CvtColor(diffFrame, grayDiff, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(grayDiff, blurredDiff, new Size(21, 21), 0);
                Cv2.Threshold(blurredDiff, thresholdFrame, settings.ThresholdValue, 255, ThresholdTypes.Binary);

                // Calculate motion percentage
                double motionPercentage = Cv2.CountNonZero(thresholdFrame) / (double)(thresholdFrame.Rows * thresholdFrame.Cols);

                if (motionPercentage > settings.Sensitivity)
                {
                    motionFrameCount++;
                    if (motionFrameCount >= settings.MinimumMotionFrames)
                    {
                        // Check if enough time has passed since last notification
                        if ((DateTime.UtcNow - lastNotification).TotalSeconds >= 30)
                        {
                            await HandleMotionDetectedAsync(camera, currentFrame, motionPercentage, settings);
                            lastNotification = DateTime.UtcNow;
                        }
                        motionFrameCount = 0;
                    }
                }
                else
                {
                    motionFrameCount = Math.Max(0, motionFrameCount - 1);
                }

                currentFrame.CopyTo(prevFrame);
                await Task.Delay(100, ct); // Process 10 frames per second
            }
        }

        private async Task HandleMotionDetectedAsync(CameraDevice camera, Mat frame, double motionPercentage, MotionDetectionSettings settings)
        {
            try
            {
                // Save detection image if enabled
                if (settings.SaveDetectionImages)
                {
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                    var imagePath = Path.Combine(settings.SavePath, $"motion_{timestamp}.jpg");
                    Directory.CreateDirectory(settings.SavePath);
                    frame.ImWrite(imagePath);
                }

                // Log the event
                await _loggingService.LogDeviceEventAsync(
                    "Camera",
                    camera.Id.ToString(),
                    "MotionDetected",
                    $"Motion detected with {motionPercentage:P2} coverage");

                // Send notification
                await _emailService.SendMotionDetectionAlertAsync(
                    camera.Name,
                    camera.Location,
                    DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "MotionDetection", "Error handling motion detection");
            }
        }

        public void Dispose()
        {
            foreach (var cts in _detectionTasks.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _detectionTasks.Clear();
        }
    }
}