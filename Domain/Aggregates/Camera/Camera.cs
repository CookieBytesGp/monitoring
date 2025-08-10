using Domain.SeedWork;
using FluentResults;
using Domain.Aggregates.Camera.ValueObjects;
using Domain.Aggregates.Camera.Entities;

namespace Domain.Aggregates.Camera
{
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
        public DateTime? UpdatedAt { get; private set; }
        public DateTime? LastActiveAt { get; private set; }

        // Collections
        public IReadOnlyCollection<CameraStream> Streams => _streams.AsReadOnly();
        public IReadOnlyCollection<CameraCapability> Capabilities => _capabilities.AsReadOnly();

        #endregion

        #region Methods

        public static Result<Camera> CreateIPCamera(
            string name,
            string location,
            string ipAddress,
            int port,
            string username = null,
            string password = null)
        {
            var result = new Result<Camera>();

            // Validate name within aggregate
            var nameValidation = ValidateName(name);
            if (nameValidation.IsFailed)
            {
                result.WithErrors(nameValidation.Errors);
            }

            var locationResult = CameraLocation.Create(location);
            if (locationResult.IsFailed)
            {
                result.WithErrors(locationResult.Errors);
            }

            var networkResult = CameraNetwork.Create(ipAddress, port, username, password);
            if (networkResult.IsFailed)
            {
                result.WithErrors(networkResult.Errors);
            }

            if (result.IsFailed)
            {
                return result;
            }

            var camera = new Camera(
                name,
                locationResult.Value,
                networkResult.Value,
                CameraType.IP);

            camera.SetDefaultConfiguration();

            result.WithValue(camera);
            return result;
        }

        public static Result<Camera> CreateRTSPCamera(
            string name,
            string location,
            string rtspUrl,
            string username = null,
            string password = null)
        {
            var result = new Result<Camera>();

            // Validate name within aggregate
            var nameValidation = ValidateName(name);
            if (nameValidation.IsFailed)
            {
                result.WithErrors(nameValidation.Errors);
            }

            var locationResult = CameraLocation.Create(location);
            if (locationResult.IsFailed)
            {
                result.WithErrors(locationResult.Errors);
            }

            var networkResult = CameraNetwork.CreateFromRtspUrl(rtspUrl, username, password);
            if (networkResult.IsFailed)
            {
                result.WithErrors(networkResult.Errors);
            }

            if (result.IsFailed)
            {
                return result;
            }

            var camera = new Camera(
                name,
                locationResult.Value,
                networkResult.Value,
                CameraType.RTSP);

            camera.SetDefaultConfiguration();

            result.WithValue(camera);
            return result;
        }

        public static Result<Camera> CreateONVIFCamera(
            string name,
            string location,
            string ipAddress,
            int port,
            string username,
            string password)
        {
            var result = new Result<Camera>();

            // Validate name within aggregate
            var nameValidation = ValidateName(name);
            if (nameValidation.IsFailed)
            {
                result.WithErrors(nameValidation.Errors);
            }

            var locationResult = CameraLocation.Create(location);
            if (locationResult.IsFailed)
            {
                result.WithErrors(locationResult.Errors);
            }

            var networkResult = CameraNetwork.Create(ipAddress, port, username, password);
            if (networkResult.IsFailed)
            {
                result.WithErrors(networkResult.Errors);
            }

            if (result.IsFailed)
            {
                return result;
            }

            var camera = new Camera(
                name,
                locationResult.Value,
                networkResult.Value,
                CameraType.ONVIF);

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
            switch (Type)
            {
                case CameraType.IP:
                case CameraType.ONVIF:
                    AddDefaultIPStreams();
                    break;
                case CameraType.RTSP:
                    AddDefaultRTSPStreams();
                    break;
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

        #endregion
    }
}
