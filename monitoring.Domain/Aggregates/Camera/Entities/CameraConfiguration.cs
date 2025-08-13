using FluentResults;
using Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.SeedWork;
using System.Collections.Generic;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;

namespace Monitoring.Domain.Aggregates.Camera.Entities;

public class CameraConfiguration : Entity
{
    public string Resolution { get; private set; }
    public int FrameRate { get; private set; }
    public string VideoCodec { get; private set; }
    public int Bitrate { get; private set; }
    public bool AudioEnabled { get; private set; }
    public string AudioCodec { get; private set; }
    public string Brand { get; private set; }
    public Dictionary<string, string> AdditionalSettings { get; private set; }
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
        string brand = null,
        Dictionary<string, string> additionalSettings = null,
        MotionDetectionSettings motionDetection = null,
        RecordingSettings recording = null) : this()
    {
        ValidateCoreSettings(
            resolution: resolution,
            frameRate: frameRate,
            videoCodec: videoCodec,
            bitrate: bitrate,
            audioEnabled: audioEnabled,
            audioCodec: audioCodec);

        Resolution = resolution;
        FrameRate = frameRate;
        VideoCodec = videoCodec.ToUpperInvariant();
        Bitrate = bitrate;
        AudioEnabled = audioEnabled;
        AudioCodec = audioEnabled ? audioCodec?.ToUpperInvariant() : null;
        Brand = brand?.ToUpperInvariant();
        AdditionalSettings = additionalSettings ?? new Dictionary<string, string>();
        MotionDetection = motionDetection ?? MotionDetectionSettings.CreateDefault();
        Recording = recording ?? RecordingSettings.CreateDefault();
    }

    public static Result<CameraConfiguration> CreateDefault()
    {
        var result = new Result<CameraConfiguration>();

        try
        {
            ValidateCoreSettings(
                resolution: "1920x1080",
                frameRate: 25,
                videoCodec: "H264",
                bitrate: 2048,
                audioEnabled: false,
                audioCodec: null);
        }
        catch (Exception ex)
        {
            result.WithError(ex.Message);
            return result;
        }

        var configuration = new CameraConfiguration(
            resolution: "1920x1080",
            frameRate: 25,
            videoCodec: "H264",
            bitrate: 2048,
            audioEnabled: false
        );

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
        string brand = null,
        Dictionary<string, string> additionalSettings = null,
        MotionDetectionSettings motionDetection = null,
        RecordingSettings recording = null,
        string createdBy = "System")
    {
        var result = new Result<CameraConfiguration>();

        try
        {
            ValidateCoreSettings(
                resolution: resolution,
                frameRate: frameRate,
                videoCodec: videoCodec,
                bitrate: bitrate,
                audioEnabled: audioEnabled,
                audioCodec: audioCodec);
        }
        catch (Exception ex)
        {
            result.WithError(ex.Message);
            return result;
        }

        var configuration = new CameraConfiguration(
            resolution,
            frameRate,
            videoCodec,
            bitrate,
            audioEnabled,
            audioCodec,
            brand,
            additionalSettings,
            motionDetection,
            recording
        );

        result.WithValue(configuration);
        return result;
    }

    public static CameraConfiguration CreateHighQuality()
    {
        return new CameraConfiguration(
            resolution: "1920x1080",
            frameRate: 30,
            videoCodec: "H265",
            bitrate: 4096,
            audioEnabled: true,
            audioCodec: "AAC"
        );
    }

    public static CameraConfiguration CreateLowBandwidth()
    {
        return new CameraConfiguration(
            resolution: "640x480",
            frameRate: 15,
            videoCodec: "H264",
            bitrate: 512,
            audioEnabled: false
        );
    }

    // Business Methods
    public void UpdateVideoSettings(string resolution, int frameRate, string videoCodec, int bitrate, string updatedBy = "User")
    {
        ValidateCoreSettings(
            resolution: resolution,
            frameRate: frameRate,
            videoCodec: videoCodec,
            bitrate: bitrate,
            audioEnabled: AudioEnabled,
            audioCodec: AudioEnabled ? AudioCodec : null);

        Resolution = resolution;
        FrameRate = frameRate;
        VideoCodec = videoCodec.ToUpperInvariant();
        Bitrate = bitrate;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void UpdateAudioSettings(bool audioEnabled, string audioCodec = null, string updatedBy = "User")
    {
        if (audioEnabled && string.IsNullOrWhiteSpace(audioCodec))
            throw new InvalidOperationException("Audio codec must be specified when audio is enabled");

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

    public void UpdateBrand(string brand, string updatedBy = "User")
    {
        Brand = brand?.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void UpdateAdditionalSettings(Dictionary<string, string> additionalSettings, string updatedBy = "User")
    {
        AdditionalSettings = additionalSettings ?? new Dictionary<string, string>();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void AddAdditionalSetting(string key, string value, string updatedBy = "User")
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        AdditionalSettings[key] = value;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }


    

    public void OptimizeForBandwidth(int maxBitrate, string updatedBy = "System")
    {
        if (maxBitrate < 100)
            throw new ArgumentException("Max bitrate must be at least 100 kbps", nameof(maxBitrate));

        if (Bitrate <= maxBitrate)
            return;

        // Compute new settings first, then validate
        var newResolution = Resolution;
        var newFrameRate = FrameRate;
        var newBitrate = maxBitrate;

        if (maxBitrate < 1024)
        {
            newResolution = "640x480";
            newFrameRate = Math.Min(FrameRate, 15);
        }
        else if (maxBitrate < 2048)
        {
            newResolution = "1280x720";
            newFrameRate = Math.Min(FrameRate, 20);
        }

        ValidateCoreSettings(
            resolution: newResolution,
            frameRate: newFrameRate,
            videoCodec: VideoCodec,
            bitrate: newBitrate,
            audioEnabled: AudioEnabled,
            audioCodec: AudioCodec);

        Resolution = newResolution;
        FrameRate = newFrameRate;
        Bitrate = newBitrate;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
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
        ValidateCoreSettings(
            resolution: Resolution,
            frameRate: FrameRate,
            videoCodec: VideoCodec,
            bitrate: Bitrate,
            audioEnabled: AudioEnabled,
            audioCodec: AudioCodec);
    }

    private static bool IsHighDefinitionResolution(string resolution)
    {
        return resolution == "1920x1080" || resolution == "2560x1440" || resolution == "3840x2160";
    }

    private static void ValidateCoreSettings(
        string resolution,
        int frameRate,
        string videoCodec,
        int bitrate,
        bool audioEnabled,
        string audioCodec)
    {
        if (string.IsNullOrWhiteSpace(resolution))
            throw new ArgumentException("Resolution cannot be empty", nameof(resolution));

        if (frameRate < 1 || frameRate > 60)
            throw new ArgumentException("Frame rate must be between 1 and 60 fps", nameof(frameRate));

        if (string.IsNullOrWhiteSpace(videoCodec))
            throw new ArgumentException("Video codec cannot be empty", nameof(videoCodec));

        if (bitrate < 100 || bitrate > 50000)
            throw new ArgumentException("Bitrate must be between 100 and 50000 kbps", nameof(bitrate));

        var normalizedCodec = videoCodec.ToUpperInvariant();

        if (IsHighDefinitionResolution(resolution) && bitrate < 1024)
            throw new InvalidOperationException("High definition resolution requires at least 1024 kbps bitrate");

        if (normalizedCodec == "H265" && bitrate < 512)
            throw new InvalidOperationException("H265 codec requires at least 512 kbps bitrate for optimal quality");

        if (audioEnabled && string.IsNullOrWhiteSpace(audioCodec))
            throw new InvalidOperationException("Audio codec must be specified when audio is enabled");
    }

    public override string ToString()
    {
        var config = $"{Resolution} @ {FrameRate}fps, {VideoCodec}, {Bitrate} kbps";
        if (AudioEnabled)
            config += $", Audio: {AudioCodec}";
        
        return config;
    }
}


