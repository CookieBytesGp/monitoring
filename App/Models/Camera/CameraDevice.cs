using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using App.Models;

namespace App.Models.Camera
{
    public class CameraDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; }

        [Required]
        [StringLength(500)]
        public string StreamUrl { get; set; }

        [StringLength(100)]
        public string Credentials { get; set; }

        [StringLength(20)]
        public string Resolution { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastActive { get; set; }

        // Navigation property for motion events
        public virtual ICollection<MotionEvent> MotionEvents { get; set; }

        public CameraDevice()
        {
            IsActive = true;
            LastActive = DateTime.UtcNow;
            MotionEvents = new List<MotionEvent>();
        }
    }
} 