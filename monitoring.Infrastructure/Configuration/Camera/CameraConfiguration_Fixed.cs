// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using Monitoring.Domain.Aggregates.Camera;
// using Monitoring.Domain.Aggregates.Camera.Entities;
// using Monitoring.Domain.Aggregates.Camera.ValueObjects;

// namespace Monitoring.Infrastructure.Configuration.Camera;

// public class CameraEntityConfiguration : IEntityTypeConfiguration<Monitoring.Domain.Aggregates.Camera.Camera>
// {
//     public void Configure(EntityTypeBuilder<Monitoring.Domain.Aggregates.Camera.Camera> builder)
//     {
//         // ===============================
//         // TABLE & PRIMARY KEY
//         // ===============================
//         builder.ToTable("Cameras");
//         builder.HasKey(c => c.Id);
        
//         builder.Property(c => c.Id)
//             .HasColumnName("Id")
//             .IsRequired();

//         // ===============================
//         // BASIC PROPERTIES
//         // ===============================
//         builder.Property(c => c.Name)
//             .HasMaxLength(200)
//             .IsRequired()
//             .HasColumnName("Name");

//         builder.Property(c => c.Status)
//             .HasConversion(
//                 status => status.Value,
//                 value => Domain.Aggregates.Camera.ValueObjects.CameraStatus.FromValue(value))
//             .IsRequired()
//             .HasColumnName("Status");

//         builder.Property(c => c.Type)
//             .HasConversion(
//                 type => type.Value,
//                 value => Domain.Aggregates.Camera.ValueObjects.CameraType.FromValue(value))
//             .IsRequired()
//             .HasColumnName("Type");

//         builder.Property(c => c.UpdatedAt)
//             .IsRequired(false)
//             .HasColumnName("UpdatedAt");

//         builder.Property(c => c.LastActiveAt)
//             .IsRequired(false)
//             .HasColumnName("LastActiveAt");

//         // ===============================
//         // CAMERA LOCATION VALUE OBJECT
//         // ===============================
//         builder.OwnsOne(c => c.Location, location =>
//         {
//             location.Property(l => l.Value)
//                 .HasMaxLength(500)
//                 .IsRequired()
//                 .HasColumnName("Location_Value");

//             location.Property(l => l.Zone)
//                 .HasMaxLength(100)
//                 .IsRequired()
//                 .HasColumnName("Location_Zone");

//             location.Property(l => l.Latitude)
//                 .HasPrecision(10, 8)
//                 .IsRequired(false)
//                 .HasColumnName("Location_Latitude");

//             location.Property(l => l.Longitude)
//                 .HasPrecision(11, 8)
//                 .IsRequired(false)
//                 .HasColumnName("Location_Longitude");
//         });

//         // ===============================
//         // CAMERA NETWORK VALUE OBJECT
//         // ===============================
//         builder.OwnsOne(c => c.Network, network =>
//         {
//             network.Property(n => n.IpAddress)
//                 .HasMaxLength(45) // IPv6 support
//                 .IsRequired()
//                 .HasColumnName("Network_IpAddress");

//             network.Property(n => n.Port)
//                 .IsRequired()
//                 .HasColumnName("Network_Port");

//             network.Property(n => n.Username)
//                 .HasMaxLength(100)
//                 .IsRequired()
//                 .HasColumnName("Network_Username");

//             network.Property(n => n.Password)
//                 .HasMaxLength(255)
//                 .IsRequired()
//                 .HasColumnName("Network_Password");

//             network.Property(n => n.Type)
//                 .HasConversion(
//                     type => type.Value,
//                     value => Domain.Aggregates.Camera.ValueObjects.NetworkType.FromValue(value))
//                 .IsRequired()
//                 .HasColumnName("Network_Type");
//         });

//         // ===============================
//         // CAMERA CONNECTION INFO VALUE OBJECT
//         // ===============================
//         builder.OwnsOne(c => c.ConnectionInfo, connection =>
//         {
//             connection.Property(ci => ci.StreamUrl)
//                 .HasMaxLength(1000)
//                 .IsRequired()
//                 .HasColumnName("Connection_StreamUrl");

//             connection.Property(ci => ci.SnapshotUrl)
//                 .HasMaxLength(1000)
//                 .IsRequired(false)
//                 .HasColumnName("Connection_SnapshotUrl");

//             connection.Property(ci => ci.BackupStreamUrl)
//                 .HasMaxLength(1000)
//                 .IsRequired(false)
//                 .HasColumnName("Connection_BackupStreamUrl");

//             connection.Property(ci => ci.ConnectionType)
//                 .HasMaxLength(50)
//                 .IsRequired()
//                 .HasColumnName("Connection_Type");

//             connection.Property(ci => ci.IsConnected)
//                 .IsRequired()
//                 .HasColumnName("Connection_IsConnected");

//             connection.Property(ci => ci.ConnectedAt)
//                 .IsRequired(false)
//                 .HasColumnName("Connection_ConnectedAt");

//             connection.Property(ci => ci.LastHeartbeat)
//                 .IsRequired(false)
//                 .HasColumnName("Connection_LastHeartbeat");
//         });

//         // ===============================
//         // ENTITY RELATIONSHIPS
//         // ===============================
        
//         // Camera Streams (One-to-Many)
//         builder.HasMany<CameraStream>()
//             .WithOne()
//             .HasForeignKey("CameraId")
//             .OnDelete(DeleteBehavior.Cascade);

//         // Camera Capabilities (One-to-Many)
//         builder.HasMany<CameraCapability>()
//             .WithOne()
//             .HasForeignKey("CameraId")
//             .OnDelete(DeleteBehavior.Cascade);

//         // Camera Configuration (One-to-One)
//         builder.HasOne<CameraConfiguration>()
//             .WithOne()
//             .HasForeignKey<CameraConfiguration>("CameraId")
//             .OnDelete(DeleteBehavior.Cascade);

//         // ===============================
//         // INDEXES
//         // ===============================
//         builder.HasIndex(c => c.Name)
//             .HasDatabaseName("IX_Cameras_Name")
//             .IsUnique();

//         builder.HasIndex(c => c.Status)
//             .HasDatabaseName("IX_Cameras_Status");

//         builder.HasIndex(c => c.Type)
//             .HasDatabaseName("IX_Cameras_Type");
//     }
// }

// public class CameraStreamEntityConfiguration : IEntityTypeConfiguration<CameraStream>
// {
//     public void Configure(EntityTypeBuilder<CameraStream> builder)
//     {
//         // ===============================
//         // TABLE & PRIMARY KEY
//         // ===============================
//         builder.ToTable("CameraStreams");
//         builder.HasKey(cs => cs.Id);

//         builder.Property(cs => cs.Id)
//             .HasColumnName("Id")
//             .IsRequired();

//         // ===============================
//         // PROPERTIES
//         // ===============================
//         builder.Property(cs => cs.Quality)
//             .HasConversion(
//                 quality => quality.Value,
//                 value => Domain.Aggregates.Camera.ValueObjects.StreamQuality.FromValue(value))
//             .IsRequired()
//             .HasColumnName("Quality");

//         builder.Property(cs => cs.Url)
//             .HasMaxLength(1000)
//             .IsRequired()
//             .HasColumnName("Url");

//         builder.Property(cs => cs.IsActive)
//             .IsRequired()
//             .HasColumnName("IsActive");

//         builder.Property(cs => cs.CreatedAt)
//             .IsRequired()
//             .HasColumnName("CreatedAt");

//         builder.Property(cs => cs.UpdatedAt)
//             .IsRequired(false)
//             .HasColumnName("UpdatedAt");

//         // Foreign Key (handled by Camera aggregate)
//         builder.Property<Guid>("CameraId")
//             .HasColumnName("CameraId")
//             .IsRequired();

//         // ===============================
//         // INDEXES
//         // ===============================
//         builder.HasIndex("CameraId")
//             .HasDatabaseName("IX_CameraStreams_CameraId");

//         builder.HasIndex(cs => cs.Quality)
//             .HasDatabaseName("IX_CameraStreams_Quality");
//     }
// }

// public class CameraCapabilityEntityConfiguration : IEntityTypeConfiguration<CameraCapability>
// {
//     public void Configure(EntityTypeBuilder<CameraCapability> builder)
//     {
//         // ===============================
//         // TABLE & PRIMARY KEY
//         // ===============================
//         builder.ToTable("CameraCapabilities");
//         builder.HasKey(cc => cc.Id);

//         builder.Property(cc => cc.Id)
//             .HasColumnName("Id")
//             .IsRequired();

//         // ===============================
//         // PROPERTIES
//         // ===============================
//         builder.Property(cc => cc.Type)
//             .HasConversion(
//                 type => type.Value,
//                 value => Domain.Aggregates.Camera.ValueObjects.CapabilityType.FromValue(value))
//             .IsRequired()
//             .HasColumnName("Type");

//         builder.Property(cc => cc.IsEnabled)
//             .IsRequired()
//             .HasColumnName("IsEnabled");

//         builder.Property(cc => cc.Configuration)
//             .HasColumnType("nvarchar(max)")
//             .IsRequired(false)
//             .HasColumnName("Configuration");

//         builder.Property(cc => cc.CreatedAt)
//             .IsRequired()
//             .HasColumnName("CreatedAt");

//         builder.Property(cc => cc.UpdatedAt)
//             .IsRequired(false)
//             .HasColumnName("UpdatedAt");

//         // Foreign Key (handled by Camera aggregate)
//         builder.Property<Guid>("CameraId")
//             .HasColumnName("CameraId")
//             .IsRequired();

//         // ===============================
//         // INDEXES
//         // ===============================
//         builder.HasIndex("CameraId")
//             .HasDatabaseName("IX_CameraCapabilities_CameraId");

//         builder.HasIndex(cc => cc.Type)
//             .HasDatabaseName("IX_CameraCapabilities_Type");
//     }
// }

// public class CameraConfigurationEntityConfiguration : IEntityTypeConfiguration<CameraConfiguration>
// {
//     public void Configure(EntityTypeBuilder<CameraConfiguration> builder)
//     {
//         // ===============================
//         // TABLE & PRIMARY KEY
//         // ===============================
//         builder.ToTable("CameraConfigurations");
//         builder.HasKey(cc => cc.Id);

//         builder.Property(cc => cc.Id)
//             .HasColumnName("Id")
//             .IsRequired();

//         // ===============================
//         // BASIC PROPERTIES
//         // ===============================
//         builder.Property(cc => cc.Resolution)
//             .HasMaxLength(20)
//             .IsRequired()
//             .HasColumnName("Resolution");

//         builder.Property(cc => cc.FrameRate)
//             .IsRequired()
//             .HasColumnName("FrameRate");

//         builder.Property(cc => cc.VideoCodec)
//             .HasMaxLength(50)
//             .IsRequired()
//             .HasColumnName("VideoCodec");

//         builder.Property(cc => cc.Bitrate)
//             .IsRequired()
//             .HasColumnName("Bitrate");

//         builder.Property(cc => cc.AudioEnabled)
//             .IsRequired()
//             .HasColumnName("AudioEnabled");

//         builder.Property(cc => cc.AudioCodec)
//             .HasMaxLength(50)
//             .IsRequired(false)
//             .HasColumnName("AudioCodec");

//         builder.Property(cc => cc.Brand)
//             .HasMaxLength(100)
//             .IsRequired(false)
//             .HasColumnName("Brand");

//         builder.Property(cc => cc.AdditionalSettings)
//             .HasConversion(
//                 v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
//                 v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions)null))
//             .HasColumnType("nvarchar(max)")
//             .IsRequired(false)
//             .HasColumnName("AdditionalSettings");

//         // ===============================
//         // MOTION DETECTION VALUE OBJECT
//         // ===============================
//         builder.OwnsOne<CameraConfiguration, Domain.Aggregates.Camera.ValueObjects.MotionDetectionSettings>(
//             cc => cc.MotionDetection, 
//             motion =>
//             {
//                 motion.Property(m => m.IsEnabled)
//                     .IsRequired()
//                     .HasColumnName("MotionDetection_IsEnabled");
                    
//                 motion.Property(m => m.Sensitivity)
//                     .IsRequired()
//                     .HasColumnName("MotionDetection_Sensitivity");
                    
//                 motion.Property(m => m.DetectionZone)
//                     .HasMaxLength(1000)
//                     .IsRequired(false)
//                     .HasColumnName("MotionDetection_Zone");
//             });

//         // ===============================
//         // RECORDING SETTINGS VALUE OBJECT
//         // ===============================
//         builder.OwnsOne<CameraConfiguration, Domain.Aggregates.Camera.ValueObjects.RecordingSettings>(
//             cc => cc.Recording, 
//             recording =>
//             {
//                 recording.Property(r => r.IsEnabled)
//                     .IsRequired()
//                     .HasColumnName("Recording_IsEnabled");
                    
//                 recording.Property(r => r.Quality)
//                     .HasConversion(
//                         quality => quality.Value,
//                         value => Domain.Aggregates.Camera.ValueObjects.RecordingQuality.FromValue(value))
//                     .IsRequired()
//                     .HasColumnName("Recording_Quality");
                    
//                 recording.Property(r => r.Duration)
//                     .HasConversion(
//                         duration => duration.TotalMinutes,
//                         minutes => TimeSpan.FromMinutes(minutes))
//                     .IsRequired()
//                     .HasColumnName("Recording_Duration");
                    
//                 recording.Property(r => r.StoragePath)
//                     .HasMaxLength(500)
//                     .IsRequired(false)
//                     .HasColumnName("Recording_StoragePath");
//             });

//         // Foreign Key (handled by Camera aggregate)
//         builder.Property<Guid>("CameraId")
//             .HasColumnName("CameraId")
//             .IsRequired();

//         // ===============================
//         // INDEXES
//         // ===============================
//         builder.HasIndex("CameraId")
//             .HasDatabaseName("IX_CameraConfigurations_CameraId")
//             .IsUnique();

//         builder.HasIndex(cc => cc.Resolution)
//             .HasDatabaseName("IX_CameraConfigurations_Resolution");
//     }
// }
