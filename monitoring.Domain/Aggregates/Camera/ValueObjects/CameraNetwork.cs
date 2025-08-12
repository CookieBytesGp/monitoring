using System;
using Monitoring.Domain.SeedWork;
using FluentResults;
using System.Net;
using System.Text.RegularExpressions;

namespace Domain.Aggregates.Camera.ValueObjects;

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
        ValidateCoreSettings(ipAddress, port, type, username, password);

        IpAddress = ipAddress.Trim();
        Port = port;
        Username = username?.Trim();
        Password = password?.Trim();
        Type = type;
    }

    public static Result<CameraNetwork> Create(string ipAddress, int port, string username = null, string password = null)
    {
        var result = new Result<CameraNetwork>();

        try
        {
            ValidateCoreSettings(ipAddress, port, NetworkType.HTTP, username, password);
            var cameraNetwork = new CameraNetwork(ipAddress, port, username, password, NetworkType.HTTP);
            result.WithValue(cameraNetwork);
        }
        catch (Exception ex)
        {
            result.WithError(ex.Message);
        }
        return result;
    }

    public static Result<CameraNetwork> CreateFromRtspUrl(string rtspUrl, string username = null, string password = null)
    {
        var result = new Result<CameraNetwork>();

        try
        {
            if (string.IsNullOrWhiteSpace(rtspUrl))
                throw new ArgumentException("RTSP URL cannot be empty", nameof(rtspUrl));

            var uri = new Uri(rtspUrl);
            if (!uri.Scheme.Equals("rtsp", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("RTSP URL must start with 'rtsp://'", nameof(rtspUrl));

            var extractedUsername = !string.IsNullOrWhiteSpace(username)
                ? username
                : (string.IsNullOrWhiteSpace(uri.UserInfo) ? null : uri.UserInfo.Split(':')[0]);

            var extractedPassword = !string.IsNullOrWhiteSpace(password)
                ? password
                : (string.IsNullOrWhiteSpace(uri.UserInfo) ? null : (uri.UserInfo.Contains(':') ? uri.UserInfo.Split(':')[1] : null));

            var host = uri.Host;
            var parsedPort = uri.Port > 0 ? uri.Port : 554;

            ValidateCoreSettings(host, parsedPort, NetworkType.RTSP, extractedUsername, extractedPassword);

            var network = new CameraNetwork(host, parsedPort, extractedUsername, extractedPassword, NetworkType.RTSP);
            result.WithValue(network);
        }
        catch (Exception ex)
        {
            result.WithError(ex.Message);
        }

        return result;
    }

    public static CameraNetwork FromRtspUrl(string rtspUrl, string username = null, string password = null)
    {
        if (string.IsNullOrWhiteSpace(rtspUrl))
            throw new ArgumentException("RTSP URL cannot be empty", nameof(rtspUrl));

        var uri = new Uri(rtspUrl);
        
        if (uri.Scheme.ToLower() != "rtsp")
            throw new ArgumentException("URL must be RTSP protocol", nameof(rtspUrl));

        var extractedUsername = !string.IsNullOrWhiteSpace(username)
            ? username
            : (string.IsNullOrWhiteSpace(uri.UserInfo) ? null : uri.UserInfo.Split(':')[0]);
        var extractedPassword = !string.IsNullOrWhiteSpace(password)
            ? password
            : (string.IsNullOrWhiteSpace(uri.UserInfo) ? null : (uri.UserInfo.Contains(':') ? uri.UserInfo.Split(':')[1] : null));

        var host = uri.Host;
        var parsedPort = uri.Port > 0 ? uri.Port : 554;

        ValidateCoreSettings(host, parsedPort, NetworkType.RTSP, extractedUsername, extractedPassword);

        return new CameraNetwork(host, parsedPort, extractedUsername, extractedPassword, NetworkType.RTSP);
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

    // Validation similar to CameraConfiguration
    private static void ValidateCoreSettings(string ipOrHost, int port, NetworkType type, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(ipOrHost))
            throw new ArgumentException("IP Address or hostname cannot be empty", nameof(ipOrHost));

        if (!IsValidIpAddress(ipOrHost) && !IsValidHostname(ipOrHost))
            throw new ArgumentException("Invalid IP Address or hostname", nameof(ipOrHost));

        if (port < 1 || port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535", nameof(port));

        if (type == NetworkType.ONVIF)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("ONVIF cameras require username and password");
        }
    }

    private void ValidateConfiguration()
    {
        ValidateCoreSettings(IpAddress, Port, Type, Username, Password);
    }
}

public enum NetworkType
{
    HTTP = 1,
    RTSP = 2,
    ONVIF = 3
}
