using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using App.Models.Camera;

namespace App.Models
{
    public class MotionEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CameraId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Camera Name")]
        public string CameraName { get; set; }

        [Required]
        [Display(Name = "Detection Time")]
        public DateTime Timestamp { get; set; }

        [Required]
        [Range(0, 100)]
        [Display(Name = "Motion %")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal MotionPercentage { get; set; }

        [StringLength(500)]
        [Display(Name = "Image Path")]
        public string ImagePath { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }

        [Display(Name = "Detection Region X")]
        public int DetectionX { get; set; }

        [Display(Name = "Detection Region Y")]
        public int DetectionY { get; set; }

        [Display(Name = "Detection Region Width")]
        public int DetectionWidth { get; set; }

        [Display(Name = "Detection Region Height")]
        public int DetectionHeight { get; set; }

        [NotMapped]
        [Display(Name = "Detection Region")]
        public Rectangle DetectionRegion
        {
            get => new Rectangle(DetectionX, DetectionY, DetectionWidth, DetectionHeight);
            set
            {
                DetectionX = value.X;
                DetectionY = value.Y;
                DetectionWidth = value.Width;
                DetectionHeight = value.Height;
            }
        }

        [Range(0, 1)]
        [Column(TypeName = "decimal(4,3)")]
        public decimal Sensitivity { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public bool Acknowledged { get; set; }

        [Display(Name = "Acknowledged At")]
        public DateTime? AcknowledgedAt { get; set; }

        [StringLength(100)]
        [Display(Name = "Acknowledged By")]
        public string AcknowledgedBy { get; set; }

        [Display(Name = "Processing Status")]
        [StringLength(50)]
        public string ProcessingStatus { get; set; } = "Pending";

        [Display(Name = "Error Message")]
        [StringLength(2000)]
        public string ProcessingError { get; set; }

        [Display(Name = "Last Processed")]
        public DateTime? LastProcessedAt { get; set; }

        [Display(Name = "Duration (ms)")]
        public int? ProcessingDurationMs { get; set; }

        [Display(Name = "File Size (bytes)")]
        public long? ImageSizeBytes { get; set; }

        [Display(Name = "Image Resolution")]
        [StringLength(20)]
        public string ImageResolution { get; set; }

        [Display(Name = "Blur Level")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? BlurLevel { get; set; }

        [Display(Name = "Brightness Level")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? BrightnessLevel { get; set; }

        [Display(Name = "Is False Positive")]
        public bool IsFalsePositive { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual CameraDevice Camera { get; set; }
        public virtual ICollection<ImageProcessingHistory> ProcessingHistory { get; set; }

        public MotionEvent()
        {
            Timestamp = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            Acknowledged = false;
            ProcessingStatus = "Pending";
            Sensitivity = 0.3M; // Default sensitivity
            ProcessingHistory = new List<ImageProcessingHistory>();
        }

        /// <summary>
        /// Updates the processing status and related fields
        /// </summary>
        public void UpdateProcessingStatus(string status, string error = null, int? durationMs = null)
        {
            ProcessingStatus = status;
            ProcessingError = error;
            ProcessingDurationMs = durationMs;
            LastProcessedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Acknowledges the motion event
        /// </summary>
        public void Acknowledge(string userId)
        {
            if (!Acknowledged)
            {
                Acknowledged = true;
                AcknowledgedAt = DateTime.UtcNow;
                AcknowledgedBy = userId;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Updates the image analysis results
        /// </summary>
        public void UpdateImageAnalysis(decimal? blurLevel, decimal? brightness, string resolution, long? fileSize)
        {
            BlurLevel = blurLevel;
            BrightnessLevel = brightness;
            ImageResolution = resolution;
            ImageSizeBytes = fileSize;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}