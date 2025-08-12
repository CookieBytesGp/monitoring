using FluentResults;
using Monitoring.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monitoring.Domain.Aggregates.Camera.ValueObjects
{
    /// <summary>
    /// Value Object برای نگهداری اطلاعات اتصال دوربین
    /// شامل URL های مختلف و وضعیت اتصال
    /// </summary>
    public class CameraConnectionInfo : ValueObject
    {
        public string StreamUrl { get; private set; }
        public string SnapshotUrl { get; private set; }
        public string BackupStreamUrl { get; private set; }
        public bool IsConnected { get; private set; }
        public DateTime ConnectedAt { get; private set; }
        public DateTime? LastHeartbeat { get; private set; }
        public string ConnectionType { get; private set; }
        public Dictionary<string, string> AdditionalInfo { get; private set; }

        #region Constructors
        private CameraConnectionInfo()
        {
            // Required by EF Core and serialization
            AdditionalInfo = new Dictionary<string, string>();
        }

        private CameraConnectionInfo(
            string streamUrl, 
            string snapshotUrl, 
            bool isConnected, 
            string connectionType,
            string backupStreamUrl = null)
        {
            StreamUrl = streamUrl?.Trim();
            SnapshotUrl = snapshotUrl?.Trim();
            BackupStreamUrl = backupStreamUrl?.Trim();
            IsConnected = isConnected;
            ConnectedAt = DateTime.UtcNow;
            LastHeartbeat = isConnected ? DateTime.UtcNow : null;
            ConnectionType = connectionType?.Trim();
            AdditionalInfo = new Dictionary<string, string>();
        }
        #endregion

        #region Factory
        public static Result<CameraConnectionInfo> Create(
            string streamUrl, 
            string snapshotUrl, 
            bool isConnected, 
            string connectionType,
            string backupStreamUrl = null)
        {
            var result = new Result<CameraConnectionInfo>();

            if (string.IsNullOrWhiteSpace(streamUrl))
            {
                result.WithError("Stream URL cannot be empty");
                return result;
            }

            if (string.IsNullOrWhiteSpace(connectionType))
            {
                result.WithError("Connection type cannot be empty");
                return result;
            }

            var connectionInfo = new CameraConnectionInfo(
                streamUrl, 
                snapshotUrl, 
                isConnected, 
                connectionType, 
                backupStreamUrl);

            result.WithValue(connectionInfo);
            return result;
        }
        #endregion

        #region Methods
        public CameraConnectionInfo UpdateHeartbeat()
        {
            var updated = new CameraConnectionInfo(StreamUrl, SnapshotUrl, IsConnected, ConnectionType, BackupStreamUrl)
            {
                ConnectedAt = this.ConnectedAt,
                LastHeartbeat = DateTime.UtcNow,
                AdditionalInfo = new Dictionary<string, string>(this.AdditionalInfo)
            };
            return updated;
        }

        public CameraConnectionInfo SetAsDisconnected()
        {
            var disconnected = new CameraConnectionInfo(StreamUrl, SnapshotUrl, false, ConnectionType, BackupStreamUrl)
            {
                ConnectedAt = this.ConnectedAt,
                LastHeartbeat = this.LastHeartbeat,
                AdditionalInfo = new Dictionary<string, string>(this.AdditionalInfo)
            };
            return disconnected;
        }

        public CameraConnectionInfo AddInfo(string key, string value)
        {
            var updated = new CameraConnectionInfo(StreamUrl, SnapshotUrl, IsConnected, ConnectionType, BackupStreamUrl)
            {
                ConnectedAt = this.ConnectedAt,
                LastHeartbeat = this.LastHeartbeat,
                AdditionalInfo = new Dictionary<string, string>(this.AdditionalInfo)
            };
            updated.AdditionalInfo[key] = value;
            return updated;
        }

        public bool IsHealthy(TimeSpan maxHeartbeatAge)
        {
            if (!IsConnected) return false;
            if (!LastHeartbeat.HasValue) return false;
            
            return DateTime.UtcNow - LastHeartbeat.Value <= maxHeartbeatAge;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StreamUrl?.ToUpperInvariant();
            yield return SnapshotUrl?.ToUpperInvariant();
            yield return BackupStreamUrl?.ToUpperInvariant();
            yield return IsConnected;
            yield return ConnectionType?.ToUpperInvariant();
        }

        public override string ToString()
        {
            var status = IsConnected ? "Connected" : "Disconnected";
            var info = $"[{ConnectionType}] {status}";
            
            if (IsConnected && LastHeartbeat.HasValue)
            {
                info += $" (Last heartbeat: {LastHeartbeat:HH:mm:ss})";
            }
            
            return info;
        }
        #endregion
    }
}
