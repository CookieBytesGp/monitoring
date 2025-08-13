using Domain.Aggregates.Camera.ValueObjects;
using Domain.Aggregates.Camera.Entities;
using Monitoring.Domain.SeedWork;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.Aggregates.Camera.Entities;
using FluentResults;

namespace Monitoring.Domain.Aggregates.Camera;

public class Camera : AggregateRoot
{
    private readonly List<CameraStream> _streams = new();
    private readonly List<CameraCapability> _capabilities = new();

    #region Ctor

    private Camera() : base()
    {
        
    }

    private Camera(
    string name,
    CameraLocation location,
    CameraNetwork network,
    CameraType type) : this()
    {
    ValidateName(name);
    Name = name;
    Location = location;
    Network = network;
    Type = type;
    Status = CameraStatus.Inactive;
    }

    #endregion

    #region Properties

    public string Name { get; private set; }
    public CameraLocation Location { get; private set; }
    public CameraNetwork Network { get; private set; }
    public CameraType Type { get; private set; }
    public CameraStatus Status { get; private set; }
    public CameraConfiguration Configuration { get; private set; }
    public CameraConnectionInfo ConnectionInfo { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastActiveAt { get; private set; }

    // Collections
    public IReadOnlyCollection<CameraStream> Streams => _streams.AsReadOnly();
    public IReadOnlyCollection<CameraCapability> Capabilities => _capabilities.AsReadOnly();

    #endregion

    #region Methods

    public void SetStatus(CameraStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        
        // اگر دوربین فعال شد، LastActiveAt را بروزرسانی کن
        if (status == CameraStatus.Active)
        {
            LastActiveAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// تنظیم اطلاعات اتصال دوربین
    /// </summary>
    /// <param name="connectionInfo">اطلاعات اتصال</param>
    public Result SetConnectionInfo(CameraConnectionInfo connectionInfo)
    {
        if (connectionInfo == null)
            return Result.Fail("Connection info cannot be null");

        ConnectionInfo = connectionInfo;
        UpdatedAt = DateTime.UtcNow;

        // بروزرسانی وضعیت دوربین بر اساس اتصال
        if (connectionInfo.IsConnected)
        {
            SetStatus(CameraStatus.Active);
        }
        else
        {
            SetStatus(CameraStatus.Error);
        }

        return Result.Ok();
    }

    /// <summary>
    /// بروزرسانی heartbeat اتصال
    /// </summary>
    public Result UpdateConnectionHeartbeat()
    {
        if (ConnectionInfo == null)
            return Result.Fail("No connection info available");

        if (!ConnectionInfo.IsConnected)
            return Result.Fail("Camera is not connected");

        ConnectionInfo = ConnectionInfo.UpdateHeartbeat();
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Ok();
    }

    /// <summary>
    /// قطع اتصال دوربین
    /// </summary>
    public Result DisconnectCamera()
    {
        if (ConnectionInfo != null)
        {
            ConnectionInfo = ConnectionInfo.SetAsDisconnected();
        }

        SetStatus(CameraStatus.Inactive);
        return Result.Ok();
    }

    /// <summary>
    /// بررسی سلامت اتصال دوربین
    /// </summary>
    /// <param name="maxHeartbeatAge">حداکثر مدت زمان مجاز برای آخرین heartbeat</param>
    /// <returns>وضعیت سلامت اتصال</returns>
    public bool IsConnectionHealthy(TimeSpan maxHeartbeatAge)
    {
        if (ConnectionInfo == null) return false;
        return ConnectionInfo.IsHealthy(maxHeartbeatAge);
    }

    public static Result<Camera> Create(
        string name,
        string location,
        string ipAddress,
        int port,
        string username,
        string password,
        CameraType type)
    {
        var result = new Result<Camera>();

        var nameValidation = ValidateName(name);
        if (nameValidation.IsFailed)
            result.WithErrors(nameValidation.Errors);

        var locationResult = CameraLocation.Create(location);
        if (locationResult.IsFailed)
            result.WithErrors(locationResult.Errors);

        CameraNetwork network = null;
        if (type == CameraType.RTSP)
        {
            var networkResult = CameraNetwork.CreateFromRtspUrl(ipAddress, username, password);
            if (networkResult.IsFailed)
                result.WithErrors(networkResult.Errors);
            else
                network = networkResult.Value;
        }
        else
        {
            var networkResult = CameraNetwork.Create(ipAddress, port, username, password);
            if (networkResult.IsFailed)
                result.WithErrors(networkResult.Errors);
            else
                network = networkResult.Value;
        }

        if (result.IsFailed)
            return result;

        var camera = new Camera(
            name,
            locationResult.Value,
            network,
            type);
        camera.SetDefaultConfiguration();
        result.WithValue(camera);
        return result;
    }

    // Business Methods
    public Result UpdateName(string newName)
    {
        var nameValidation = ValidateName(newName);
        if (nameValidation.IsFailed)
            return nameValidation;

        Name = newName;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }

    public void UpdateLocation(string newLocation)
    {
        Location = CameraLocation.Create(newLocation).Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Connect()
    {
        if (Status == CameraStatus.Active)
            throw new InvalidOperationException("Camera is already connected");

        Status = CameraStatus.Active;
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disconnect()
    {
        if (Status == CameraStatus.Inactive)
            throw new InvalidOperationException("Camera is already disconnected");

        Status = CameraStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsError(string errorMessage)
    {
    Status = CameraStatus.Error;
    UpdatedAt = DateTime.UtcNow;
    }

    public void AddStream(CameraStream stream)
    {
        if (_streams.Any(s => s.Quality == stream.Quality))
            throw new InvalidOperationException($"Stream with quality {stream.Quality} already exists");

        _streams.Add(stream);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStream(StreamQuality quality)
    {
        var stream = _streams.FirstOrDefault(s => s.Quality == quality);
        if (stream == null)
            throw new InvalidOperationException($"Stream with quality {quality} not found");

        _streams.Remove(stream);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCapability(CameraCapability capability)
    {
        if (_capabilities.Any(c => c.Type == capability.Type))
            throw new InvalidOperationException($"Capability {capability.Type} already exists");

        _capabilities.Add(capability);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveCapability(CapabilityType type)
    {
        var capability = _capabilities.FirstOrDefault(c => c.Type == type);
        if (capability == null)
            throw new InvalidOperationException($"Capability {type} not found");

        _capabilities.Remove(capability);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConfiguration(CameraConfiguration configuration, string updatedBy = "System")
    {
    Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateVideoSettings(string resolution, int frameRate, string videoCodec, int bitrate, string updatedBy = "User")
    {
    Configuration?.UpdateVideoSettings(resolution, frameRate, videoCodec, bitrate, updatedBy);
    UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAudioSettings(bool audioEnabled, string audioCodec = null, string updatedBy = "User")
    {
    Configuration?.UpdateAudioSettings(audioEnabled, audioCodec, updatedBy);
    UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateHeartbeat()
    {
        if (Status != CameraStatus.Active)
            return;

        LastActiveAt = DateTime.UtcNow;
    }

    public bool IsOnline()
    {
        if (Status != CameraStatus.Active || !LastActiveAt.HasValue)
            return false;

        // Consider camera offline if no heartbeat in last 2 minutes
        return DateTime.UtcNow.Subtract(LastActiveAt.Value).TotalMinutes < 2;
    }

    public bool HasCapability(CapabilityType type)
    {
    return _capabilities.Any(c => c.Type == type && c.IsEnabled);
    }

    public CameraStream GetStreamByQuality(StreamQuality quality)
    {
    return _streams.FirstOrDefault(s => s.Quality == quality);
    }

    // Private Methods
    private void SetDefaultConfiguration()
    {
        Configuration = CameraConfiguration.CreateDefault().Value;
        
        // Add default streams based on camera type
        if (Type == CameraType.IP || Type == CameraType.ONVIF)
        {
            AddDefaultIPStreams();
        }
        else if (Type == CameraType.RTSP)
        {
            AddDefaultRTSPStreams();
        }
    }

    private void AddDefaultIPStreams()
    {
    _streams.Add(CameraStream.Create(StreamQuality.High, $"http://{Network.IpAddress}:{Network.Port}/video1"));
    _streams.Add(CameraStream.Create(StreamQuality.Medium, $"http://{Network.IpAddress}:{Network.Port}/video2"));
    _streams.Add(CameraStream.Create(StreamQuality.Low, $"http://{Network.IpAddress}:{Network.Port}/video3"));
    }

    private void AddDefaultRTSPStreams()
    {
        var baseUrl = Network.HasCredentials
            ? $"rtsp://{Network.Username}:{Network.Password}@{Network.IpAddress}:{Network.Port}"
            : $"rtsp://{Network.IpAddress}:{Network.Port}";

        _streams.Add(CameraStream.Create(StreamQuality.High, $"{baseUrl}/stream1"));
        _streams.Add(CameraStream.Create(StreamQuality.Medium, $"{baseUrl}/stream2"));
    }


    private static Result ValidateName(string name)
    {
        var result = new Result();

        if (string.IsNullOrWhiteSpace(name))
        {
            result.WithError("Camera name cannot be empty");
            return result;
        }
        
        if (name.Length > 100)
        {
            result.WithError("Camera name cannot exceed 100 characters");
            return result;
        }

        if (name.Trim().Length < 2)
        {
            result.WithError("Camera name must be at least 2 characters");
            return result;
        }

        return result;
    }

    #region Strategy Support Methods

    /// <summary>
    /// بررسی اینکه آیا دوربین می‌تواند از استراتژی مشخص شده استفاده کند
    /// </summary>
    /// <param name="strategyName">نام استراتژی</param>
    /// <returns>نتیجه بررسی</returns>
    public Result<bool> CanUseStrategy(string strategyName)
    {
        if (string.IsNullOrWhiteSpace(strategyName))
            return Result.Fail<bool>("Strategy name cannot be empty");

        var strategy = strategyName.ToLower().Trim();
        
        var canUse = strategy switch
        {
            "hikvision" => CheckHikvisionCompatibility(),
            "dahua" => CheckDahuaCompatibility(), 
            "onvif" => CheckOnvifCompatibility(),
            "rtsp" => CheckRtspCompatibility(),
            "http" => CheckHttpCompatibility(),
            "usb" => CheckUsbCompatibility(),
            _ => false
        };

        return Result.Ok(canUse);
    }

    /// <summary>
    /// دریافت لیست استراتژی‌های پشتیبانی شده برای این دوربین
    /// </summary>
    /// <returns>لیست نام استراتژی‌ها به ترتیب اولویت</returns>
    public Result<List<string>> GetSupportedStrategies()
    {
        var strategies = new List<string>();

        // اولویت با SDK های اختصاصی
        if (CheckHikvisionCompatibility()) strategies.Add("hikvision");
        if (CheckDahuaCompatibility()) strategies.Add("dahua");
        
        // پروتکل‌های استاندارد
        if (CheckOnvifCompatibility()) strategies.Add("onvif");
        if (CheckRtspCompatibility()) strategies.Add("rtsp");
        if (CheckHttpCompatibility()) strategies.Add("http");
        if (CheckUsbCompatibility()) strategies.Add("usb");

        return Result.Ok(strategies);
    }

    /// <summary>
    /// بهترین استراتژی برای این دوربین
    /// </summary>
    /// <returns>نام بهترین استراتژی</returns>
    public Result<string> GetPreferredStrategy()
    {
        var supportedResult = GetSupportedStrategies();
        if (supportedResult.IsFailed || !supportedResult.Value.Any())
            return Result.Fail<string>("No supported strategy found for this camera");

        return Result.Ok(supportedResult.Value.First());
    }

    private bool CheckHikvisionCompatibility()
    {
        return !string.IsNullOrEmpty(Configuration?.Brand) && 
               Configuration.Brand.ToLower().Contains("hikvision");
    }

    private bool CheckDahuaCompatibility()
    {
        return !string.IsNullOrEmpty(Configuration?.Brand) && 
               Configuration.Brand.ToLower().Contains("dahua");
    }

    private bool CheckOnvifCompatibility()
    {
        return Type == CameraType.IP && 
               Network != null && 
               (Network.Port == 80 || Network.Port == 8080);
    }

    private bool CheckRtspCompatibility()
    {
        return Type == CameraType.IP && 
               Network != null;
    }

    private bool CheckHttpCompatibility()
    {
        return Type == CameraType.IP && 
               Network != null;
    }

    private bool CheckUsbCompatibility()
    {
        return Type == CameraType.USB;
    }

    #endregion

    #endregion
}
