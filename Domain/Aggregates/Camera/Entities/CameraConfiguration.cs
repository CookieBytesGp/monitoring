using Domain.SeedWork;
using FluentResults;
using Domain.Aggregates.Camera.ValueObjects;

namespace Domain.Aggregates.Camera.Entities
{
    public class CameraConfiguration : Entity
    {
        public string Resolution { get; private set; }
        public int FrameRate { get; private set; }
        public string VideoCodec { get; private set; }
        public int Bitrate { get; private set; }
        public bool AudioEnabled { get; private set; }
        public string AudioCodec { get; private set; }
        public MotionDetectionSettings MotionDetection { get; private set; }
        public RecordingSettings Recording { get; private set; }

        private CameraConfiguration() : base()
        {
            
        }

        private CameraConfiguration(
            string resolution,
            int frameRate,
            string videoCodec,
            int bitrate,
            bool audioEnabled = false,
            string audioCodec = null,
            MotionDetectionSettings motionDetection = null,
            RecordingSettings recording = null) : this()
        {
            Resolution = resolution;
            FrameRate = frameRate;
            VideoCodec = videoCodec.ToUpperInvariant();
            Bitrate = bitrate;
            AudioEnabled = audioEnabled;
            AudioCodec = audioCodec?.ToUpperInvariant();
            MotionDetection = motionDetection ?? MotionDetectionSettings.CreateDefault();
            Recording = recording ?? RecordingSettings.CreateDefault();
        }

        public static Result<CameraConfiguration> CreateDefault()
        {
            var result = new Result<CameraConfiguration>();

            var configuration = new CameraConfiguration(
                resolution: "1920x1080",
                frameRate: 25,
                videoCodec: "H264",
                bitrate: 2048,
                audioEnabled: false
            );

            var validationResult = configuration.Validate();
            if (validationResult.IsFailed)
            {
                result.WithErrors(validationResult.Errors);
                return result;
            }

            result.WithValue(configuration);
            return result;
        }

        public static Result<CameraConfiguration> Create(
            string resolution,
            int frameRate,
            string videoCodec,
            int bitrate,
            bool audioEnabled = false,
            string audioCodec = null,
            MotionDetectionSettings motionDetection = null,
            RecordingSettings recording = null,
            string createdBy = "System")
        {
            return new CameraConfiguration(
                Guid.NewGuid(),
                resolution,
                frameRate,
                videoCodec,
                bitrate,
                audioEnabled,
                audioCodec,
                motionDetection,
                recording,
                createdBy
            );
        }

        public static CameraConfiguration CreateHighQuality()
        {
            return new CameraConfiguration(
                id: Guid.NewGuid(),
                resolution: "1920x1080",
                frameRate: 30,
                videoCodec: "H265",
                bitrate: 4096,
                audioEnabled: true,
                audioCodec: "AAC",
                createdBy: "System"
            );
        }

        public static CameraConfiguration CreateLowBandwidth()
        {
            return new CameraConfiguration(
                id: Guid.NewGuid(),
                resolution: "640x480",
                frameRate: 15,
                videoCodec: "H264",
                bitrate: 512,
                audioEnabled: false,
                createdBy: "System"
            );
        }

        // Business Methods
        public void UpdateVideoSettings(string resolution, int frameRate, string videoCodec, int bitrate, string updatedBy = "User")
        {
            if (string.IsNullOrWhiteSpace(resolution))
                throw new ArgumentException("Resolution cannot be empty", nameof(resolution));

            if (frameRate < 1 || frameRate > 60)
                throw new ArgumentException("Frame rate must be between 1 and 60 fps", nameof(frameRate));

            if (string.IsNullOrWhiteSpace(videoCodec))
                throw new ArgumentException("Video codec cannot be empty", nameof(videoCodec));

            if (bitrate < 100 || bitrate > 50000)
                throw new ArgumentException("Bitrate must be between 100 and 50000 kbps", nameof(bitrate));

            Resolution = resolution;
            FrameRate = frameRate;
            VideoCodec = videoCodec.ToUpperInvariant();
            Bitrate = bitrate;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            ValidateConfiguration();
        }

        public void UpdateAudioSettings(bool audioEnabled, string audioCodec = null, string updatedBy = "User")
        {
            AudioEnabled = audioEnabled;
            AudioCodec = audioEnabled ? audioCodec?.ToUpperInvariant() : null;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void UpdateMotionDetection(MotionDetectionSettings motionDetection, string updatedBy = "User")
        {
            MotionDetection = motionDetection ?? throw new ArgumentNullException(nameof(motionDetection));
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void UpdateRecordingSettings(RecordingSettings recording, string updatedBy = "User")
        {
            Recording = recording ?? throw new ArgumentNullException(nameof(recording));
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void OptimizeForBandwidth(int maxBitrate, string updatedBy = "System")
        {
            if (maxBitrate < 100)
                throw new ArgumentException("Max bitrate must be at least 100 kbps", nameof(maxBitrate));

            if (Bitrate <= maxBitrate)
                return;

            // Reduce bitrate and potentially resolution
            if (maxBitrate < 1024)
            {
                Resolution = "640x480";
                FrameRate = Math.Min(FrameRate, 15);
            }
            else if (maxBitrate < 2048)
            {
                Resolution = "1280x720";
                FrameRate = Math.Min(FrameRate, 20);
            }

            Bitrate = maxBitrate;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            ValidateConfiguration();
        }

        public bool IsHighDefinition => Resolution == "1920x1080" || Resolution == "2560x1440" || Resolution == "3840x2160";

        public bool IsLowBandwidth => Bitrate < 1024;

        public string GetQualityLevel()
        {
            return Resolution switch
            {
                "3840x2160" => "4K Ultra HD",
                "2560x1440" => "QHD",
                "1920x1080" => "Full HD",
                "1280x720" => "HD",
                "640x480" => "VGA",
                _ => "Custom"
            };
        }

        // Private Methods
        private void ValidateConfiguration()
        {
            // High resolution needs sufficient bitrate
            if (IsHighDefinition && Bitrate < 1024)
                throw new InvalidOperationException("High definition resolution requires at least 1024 kbps bitrate");

            // Audio codec required if audio enabled
            if (AudioEnabled && string.IsNullOrWhiteSpace(AudioCodec))
                throw new InvalidOperationException("Audio codec must be specified when audio is enabled");

            // Validate codec combinations
            if (VideoCodec == "H265" && Bitrate < 512)
                throw new InvalidOperationException("H265 codec requires at least 512 kbps bitrate for optimal quality");
        }

        public override string ToString()
        {
            var config = $"{Resolution} @ {FrameRate}fps, {VideoCodec}, {Bitrate} kbps";
            if (AudioEnabled)
                config += $", Audio: {AudioCodec}";
            
            return config;
        }
    }

    #region Supporting Value Objects

    public class MotionDetectionSettings
    {
        public bool Enabled { get; private set; }
        public int Sensitivity { get; private set; }
        public TimeSpan PreRecordingDuration { get; private set; }
        public TimeSpan PostRecordingDuration { get; private set; }

        public MotionDetectionSettings(bool enabled, int sensitivity = 50, TimeSpan? preRecording = null, TimeSpan? postRecording = null)
        {
            if (sensitivity < 1 || sensitivity > 100)
                throw new ArgumentException("Sensitivity must be between 1 and 100", nameof(sensitivity));

            Enabled = enabled;
            Sensitivity = sensitivity;
            PreRecordingDuration = preRecording ?? TimeSpan.FromSeconds(5);
            PostRecordingDuration = postRecording ?? TimeSpan.FromSeconds(10);
        }

        public static MotionDetectionSettings CreateDefault()
        {
            return new MotionDetectionSettings(enabled: true, sensitivity: 75);
        }

        public static MotionDetectionSettings CreateDisabled()
        {
            return new MotionDetectionSettings(enabled: false);
        }
    }

    public class RecordingSettings
    {
        public bool Enabled { get; private set; }
        public RecordingMode Mode { get; private set; }
        public TimeSpan MaxDuration { get; private set; }
        public int MaxFileSize { get; private set; } // در MB
        public string StoragePath { get; private set; }

        public RecordingSettings(bool enabled, RecordingMode mode, TimeSpan maxDuration, int maxFileSize, string storagePath)
        {
            if (maxDuration <= TimeSpan.Zero)
                throw new ArgumentException("Max duration must be positive", nameof(maxDuration));

            if (maxFileSize <= 0)
                throw new ArgumentException("Max file size must be positive", nameof(maxFileSize));

            Enabled = enabled;
            Mode = mode;
            MaxDuration = maxDuration;
            MaxFileSize = maxFileSize;
            StoragePath = storagePath?.Trim();
        }

        public static RecordingSettings CreateDefault()
        {
            return new RecordingSettings(
                enabled: false,
                mode: RecordingMode.OnMotion,
                maxDuration: TimeSpan.FromMinutes(30),
                maxFileSize: 100,
                storagePath: "/recordings"
            );
        }
    }

    public enum RecordingMode
    {
        Continuous = 1,
        OnMotion = 2,
        Scheduled = 3
    }

    #endregion
}
