using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App.Models.Monitor;
using App.Models.Camera;
using App.Models.Auth;
using App.Models;

namespace App.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Auth related
        public DbSet<ApplicationUser> Users { get; set; }

        // Device related
        public DbSet<MonitorDevice> Monitors { get; set; }
        public DbSet<CameraDevice> Cameras { get; set; }

        // Event and logging related
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<MotionEvent> MotionEvents { get; set; }

        // Template related
        public DbSet<ProcessingTemplate> ProcessingTemplates { get; set; }
        public DbSet<ReportTemplate> ReportTemplates { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        // History related
        public DbSet<ReportExportHistory> ReportExportHistory { get; set; }
        public DbSet<ImageProcessingHistory> ImageProcessingHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedReportTemplates).HasMaxLength(1000);
                entity.Property(e => e.ModifiedReportTemplates).HasMaxLength(1000);
                entity.Property(e => e.Theme).HasMaxLength(20).HasDefaultValue("light");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.LastLoginAt);
                entity.HasIndex(e => new { e.FirstName, e.LastName }); // Add composite index for name searches
            });

            // EmailTemplate configuration
            builder.Entity<EmailTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Body).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.IsDefault);
                entity.HasIndex(e => e.Name).IsUnique(); // Ensure unique names
            });

            // ReportTemplate configuration
            builder.Entity<ReportTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Format).IsRequired().HasMaxLength(10);
                entity.Property(e => e.TimeRangeType).HasMaxLength(20);
                entity.Property(e => e.CameraFilter).HasMaxLength(1000);
                entity.Property(e => e.Schedule).HasMaxLength(100);
                entity.Property(e => e.EmailRecipients).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.IsScheduled);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.Name, e.Format }); // Composite index for common queries
            });

            // ReportExportHistory configuration
            builder.Entity<ReportExportHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExportedAt).IsRequired();
                entity.Property(e => e.ExportedBy).HasMaxLength(450).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(1000);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
                entity.Property(e => e.FileSizeBytes).IsRequired();

                entity.HasOne(r => r.ReportTemplate)
                    .WithMany(t => t.ReportExportHistory)
                    .HasForeignKey(r => r.ReportTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ExportedAt);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.ReportTemplateId, e.ExportedAt });
            });

            // Monitor configuration
            builder.Entity<MonitorDevice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(e => e.DisplayResolution).HasMaxLength(20);
                entity.Property(e => e.CurrentContent).HasMaxLength(1000);
                entity.Property(e => e.LastPing).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => e.IpAddress).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.LastPing);
            });

            // Camera configuration
            builder.Entity<CameraDevice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(e => e.StreamUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Credentials).HasMaxLength(100);
                entity.Property(e => e.Resolution).HasMaxLength(20);
                entity.Property(e => e.LastActive).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => e.IpAddress).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.LastActive);
                entity.HasIndex(e => new { e.Name, e.Location });
            });

            // SystemLog configuration
            builder.Entity<SystemLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Source).HasMaxLength(100);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.DeviceId).HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(50);

                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Category, e.EventType });
                entity.HasIndex(e => new { e.Severity, e.Timestamp });
            });

            // MotionEvent configuration
            builder.Entity<MotionEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CameraId).IsRequired();
                entity.Property(e => e.CameraName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.MotionPercentage).IsRequired();
                entity.Property(e => e.ImagePath).HasMaxLength(500);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.AcknowledgedBy).HasMaxLength(100);

                entity.HasOne(e => e.Camera)
                    .WithMany(c => c.MotionEvents)
                    .HasForeignKey(e => e.CameraId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.CameraId);
                entity.HasIndex(e => e.Acknowledged);
                entity.HasIndex(e => new { e.CameraId, e.Timestamp });
                entity.HasIndex(e => new { e.Acknowledged, e.Timestamp });
            });

            // ImageProcessingHistory configuration
            builder.Entity<ImageProcessingHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MotionEventId).IsRequired();
                entity.Property(e => e.ProcessedAt).IsRequired();
                entity.Property(e => e.ProcessingType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Success).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

                entity.HasOne(h => h.MotionEvent)
                    .WithMany(e => e.ProcessingHistory)
                    .HasForeignKey(h => h.MotionEventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.MotionEventId);
                entity.HasIndex(e => e.ProcessedAt);
                entity.HasIndex(e => new { e.MotionEventId, e.ProcessingType });
            });
        }
    }
}