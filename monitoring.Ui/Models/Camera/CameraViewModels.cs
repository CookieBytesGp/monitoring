namespace Monitoring.Ui.Models.Camera;

/// <summary>
/// مدل ویو برای نمایش دوربین در UI
/// </summary>
public class CameraViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// مدل ویو برای ایجاد دوربین جدید
/// </summary>
public class CreateCameraViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 554;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CameraType { get; set; } = "HTTP";
    public string StreamUrl { get; set; } = string.Empty;
    public string SnapshotUrl { get; set; } = string.Empty;
}

/// <summary>
/// مدل ویو برای اطلاعات اتصال دوربین
/// </summary>
public class CameraConnectionViewModel
{
    public string Strategy { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
}

/// <summary>
/// مدل ویو برای تست استراتژی‌ها
/// </summary>
public class CameraTestResultViewModel
{
    public string CameraName { get; set; } = string.Empty;
    public Dictionary<string, bool> StrategyResults { get; set; } = new();
    public List<string> SupportedStrategies { get; set; } = new();
}
