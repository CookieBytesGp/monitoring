using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monitoring.Domain.Aggregates.Camera;
using Domain.Aggregates.Camera.ValueObjects;
using Domain.Aggregates.Camera.Entities;

namespace Persistence.Camera.Configurations
{
    internal class CameraConfiguration : IEntityTypeConfiguration<Monitoring.Domain.Aggregates.Camera.Camera>
    {
        public void Configure(EntityTypeBuilder<Monitoring.Domain.Aggregates.Camera.Camera> builder)
        {
            // Table Configuration
            builder.ToTable("Cameras");
            
            // Primary Key
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            // Name Property (String)
            builder
                .Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired(true)
                .HasColumnName("Name");

            // Location Value Object
            builder.OwnsOne(p => p.Location, location =>
            {
                location.Property(l => l.Value)
                    .HasMaxLength(200)
                    .IsRequired(true)
                    .HasColumnName("Location");
                    
                location.Property(l => l.Zone)
                    .HasMaxLength(100)
                    .IsRequired(false)
                    .HasColumnName("LocationZone");
                    
                location.Property(l => l.Latitude)
                    .HasColumnType("decimal(10,8)")
                    .IsRequired(false)
                    .HasColumnName("Latitude");
                    
                location.Property(l => l.Longitude)
                    .HasColumnType("decimal(11,8)")
                    .IsRequired(false)
                    .HasColumnName("Longitude");
            });

            // Network Value Object
            builder.OwnsOne(p => p.Network, network =>
            {
                network.Property(n => n.IpAddress)
                    .HasMaxLength(45)
                    .IsRequired(true)
                    .HasColumnName("IpAddress");
                    
                network.Property(n => n.Port)
                    .IsRequired(true)
                    .HasColumnName("Port");
                    
                network.Property(n => n.Username)
                    .HasMaxLength(100)
                    .IsRequired(false)
                    .HasColumnName("Username");
                    
                network.Property(n => n.Password)
                    .HasMaxLength(100)
                    .IsRequired(false)
                    .HasColumnName("Password");
                    
                network.Property(n => n.Type)
                    .HasConversion<int>()
                    .IsRequired(true)
                    .HasColumnName("NetworkType");
            });

            // Enum Properties
            builder.Property(p => p.Type)
                .HasConversion<int>()
                .IsRequired(true)
                .HasColumnName("CameraType");
                
            builder.Property(p => p.Status)
                .HasConversion<int>()
                .IsRequired(true)
                .HasColumnName("Status");

            // Date Properties
            builder.Property(p => p.CreatedAt)
                .IsRequired(true)
                .HasColumnName("CreatedAt");
                
            builder.Property(p => p.LastActiveAt)
                .IsRequired(false)
                .HasColumnName("LastActiveAt");
                
            builder.Property(p => p.UpdatedAt)
                .IsRequired(false)
                .HasColumnName("UpdatedAt");

            // Configuration Entity (Owned Entity)
            builder.OwnsOne(p => p.Configuration, config =>
            {
                config.Property(c => c.Resolution)
                    .HasMaxLength(20)
                    .IsRequired(true)
                    .HasColumnName("Configuration_Resolution");
                    
                config.Property(c => c.FrameRate)
                    .IsRequired(true)
                    .HasColumnName("Configuration_FrameRate");
                    
                config.Property(c => c.VideoCodec)
                    .HasMaxLength(50)
                    .IsRequired(true)
                    .HasColumnName("Configuration_VideoCodec");
                    
                config.Property(c => c.Bitrate)
                    .IsRequired(true)
                    .HasColumnName("Configuration_Bitrate");
                    
                config.Property(c => c.AudioEnabled)
                    .IsRequired(true)
                    .HasColumnName("Configuration_AudioEnabled");
                    
                config.Property(c => c.AudioCodec)
                    .HasMaxLength(50)
                    .IsRequired(false)
                    .HasColumnName("Configuration_AudioCodec");

                // Motion Detection Settings (Owned within Configuration)
                config.OwnsOne(c => c.MotionDetection, motion =>
                {
                    motion.Property(m => m.Enabled)
                        .IsRequired(true)
                        .HasColumnName("Configuration_MotionDetection_Enabled");
                        
                    motion.Property(m => m.Sensitivity)
                        .IsRequired(true)
                        .HasColumnName("Configuration_MotionDetection_Sensitivity");
                        
                    motion.Property(m => m.PreRecordingDuration)
                        .IsRequired(true)
                        .HasColumnName("Configuration_MotionDetection_PreRecordingDuration");
                        
                    motion.Property(m => m.PostRecordingDuration)
                        .IsRequired(true)
                        .HasColumnName("Configuration_MotionDetection_PostRecordingDuration");
                });

                // Recording Settings (Owned within Configuration)
                config.OwnsOne(c => c.Recording, recording =>
                {
                    recording.Property(r => r.Enabled)
                        .IsRequired(true)
                        .HasColumnName("Configuration_Recording_Enabled");
                        
                    recording.Property(r => r.Mode)
                        .IsRequired(true)
                        .HasColumnName("Configuration_Recording_Mode")
                        .HasConversion<int>();
                        
                    recording.Property(r => r.MaxDuration)
                        .IsRequired(true)
                        .HasColumnName("Configuration_Recording_MaxDuration");
                        
                    recording.Property(r => r.MaxFileSize)
                        .IsRequired(true)
                        .HasColumnName("Configuration_Recording_MaxFileSize");
                        
                    recording.Property(r => r.StoragePath)
                        .HasMaxLength(500)
                        .IsRequired(false)
                        .HasColumnName("Configuration_Recording_StoragePath");
                });
            });

            // Streams Collection (Owned Many - Part of Camera Aggregate)
            builder.OwnsMany(p => p.Streams, streams =>
            {
                streams.WithOwner().HasForeignKey("CameraId");
                streams.Property<Guid>("CameraId");
                streams.HasKey("CameraId", "Quality");
                
                streams.Property(s => s.Quality)
                    .IsRequired(true)
                    .HasConversion<int>();
                    
                streams.Property(s => s.Url)
                    .HasMaxLength(500)
                    .IsRequired(true);
                    
                streams.Property(s => s.IsActive)
                    .IsRequired(true);
                    
                streams.Property(s => s.CreatedAt)
                    .IsRequired(true);
                    
                streams.Property(s => s.UpdatedAt)
                    .IsRequired(false);
                    
                streams.ToTable("Camera_Streams");
            });

            // Capabilities Collection (Owned Many - Part of Camera Aggregate)
            builder.OwnsMany(p => p.Capabilities, capabilities =>
            {
                capabilities.WithOwner().HasForeignKey("CameraId");
                capabilities.Property<Guid>("CameraId");
                capabilities.HasKey("CameraId", "Type");
                
                capabilities.Property(c => c.Type)
                    .IsRequired(true)
                    .HasConversion<int>();
                    
                capabilities.Property(c => c.IsEnabled)
                    .IsRequired(true);
                    
                capabilities.Property(c => c.Configuration)
                    .HasMaxLength(2000)
                    .IsRequired(false);
                    
                capabilities.Property(c => c.CreatedAt)
                    .IsRequired(true);
                    
                capabilities.Property(c => c.UpdatedAt)
                    .IsRequired(false);
                    
                capabilities.ToTable("Camera_Capabilities");
            });

            // Domain Events Ignored
            builder.Ignore(p => p.DomainEvents);

            // Indexes
            builder.HasIndex(p => p.Name)
                .IsUnique()
                .HasDatabaseName("IX_Cameras_Name");
                
            builder.HasIndex("Network_IpAddress", "Network_Port")
                .IsUnique()
                .HasDatabaseName("IX_Cameras_Network");
                
            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_Cameras_Status");
                
            builder.HasIndex(p => p.Type)
                .HasDatabaseName("IX_Cameras_Type");
        }
    }
}
