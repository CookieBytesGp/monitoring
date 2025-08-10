using Domain.SeedWork;
using FluentResults;
using System.Net;
using System.Text.RegularExpressions;

namespace Domain.Aggregates.Camera.ValueObjects
{
    public class CameraNetwork : ValueObject
    {
        public string IpAddress { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public NetworkType Type { get; private set; }

        private CameraNetwork()
        {
            
        }

        private CameraNetwork(string ipAddress, int port, string username = null, string password = null, NetworkType type = NetworkType.HTTP)
        {
            IpAddress = ipAddress.Trim();
            Port = port;
            Username = username?.Trim();
            Password = password?.Trim();
            Type = type;
        }

        public static Result<CameraNetwork> Create(string ipAddress, int port, string username = null, string password = null)
        {
            var result = new Result<CameraNetwork>();

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                result.WithError("IP Address cannot be empty");
                return result;
            }

            if (!IsValidIpAddress(ipAddress) && !IsValidHostname(ipAddress))
            {
                result.WithError("Invalid IP Address or hostname");
                return result;
            }

            if (port < 1 || port > 65535)
            {
                result.WithError("Port must be between 1 and 65535");
                return result;
            }

            var cameraNetwork = new CameraNetwork(ipAddress, port, username, password, NetworkType.HTTP);
            result.WithValue(cameraNetwork);
            return result;
        }

        public static Result<CameraNetwork> CreateFromRtspUrl(string rtspUrl, string username = null, string password = null)
        {
            var result = new Result<CameraNetwork>();

            if (string.IsNullOrWhiteSpace(rtspUrl))
            {
                result.WithError("RTSP URL cannot be empty");
                return result;
            }

            if (!rtspUrl.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase))
            {
                result.WithError("RTSP URL must start with 'rtsp://'");
                return result;
            }
                throw new ArgumentException("ONVIF cameras require username and password");

            return new CameraNetwork(ipAddress, port, username, password, NetworkType.ONVIF);
        }

        public static CameraNetwork FromRtspUrl(string rtspUrl, string username = null, string password = null)
        {
            if (string.IsNullOrWhiteSpace(rtspUrl))
                throw new ArgumentException("RTSP URL cannot be empty", nameof(rtspUrl));

            var uri = new Uri(rtspUrl);
            
            if (uri.Scheme.ToLower() != "rtsp")
                throw new ArgumentException("URL must be RTSP protocol", nameof(rtspUrl));

            var extractedUsername = username ?? uri.UserInfo?.Split(':')[0];
            var extractedPassword = password ?? uri.UserInfo?.Split(':')[1];

            return new CameraNetwork(uri.Host, uri.Port, extractedUsername, extractedPassword, NetworkType.RTSP);
        }

        public bool HasCredentials => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

        public string GetBaseUrl()
        {
            return Type switch
            {
                NetworkType.HTTP => $"http://{IpAddress}:{Port}",
                NetworkType.RTSP => HasCredentials ? $"rtsp://{Username}:{Password}@{IpAddress}:{Port}" 
                                                   : $"rtsp://{IpAddress}:{Port}",
                NetworkType.ONVIF => $"http://{IpAddress}:{Port}/onvif",
                _ => throw new InvalidOperationException($"Unknown network type: {Type}")
            };
        }

        public string GetAuthHeader()
        {
            if (!HasCredentials)
                return null;

            var credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{Username}:{Password}"));
            
            return $"Basic {credentials}";
        }

        private static bool IsValidIpAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }

        private static bool IsValidHostname(string hostname)
        {
            const string hostnamePattern = @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$";
            return Regex.IsMatch(hostname, hostnamePattern);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IpAddress.ToLowerInvariant();
            yield return Port;
            yield return Username?.ToLowerInvariant();
            yield return Type;
        }

        public override string ToString()
        {
            return $"{IpAddress}:{Port}" + (HasCredentials ? $" (Auth: {Username})" : "");
        }
    }

    public enum NetworkType
    {
        HTTP = 1,
        RTSP = 2,
        ONVIF = 3
    }
}
