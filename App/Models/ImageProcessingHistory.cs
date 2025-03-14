using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models
{
    public class ImageProcessingHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MotionEventId { get; set; }

        [ForeignKey("MotionEventId")]
        public MotionEvent MotionEvent { get; set; }

        [Required]
        public DateTime ProcessedAt { get; set; }

        [Required]
        [MaxLength(500)]
        public string ProcessedBy { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string ImagePath { get; set; }

        [MaxLength(50)]
        public string ProcessingType { get; set; } // Added property with MaxLength
        public bool Success { get; set; }
        
        [MaxLength(2000)]
        public string ErrorMessage { get; set; } // Added property with MaxLength
    }
}