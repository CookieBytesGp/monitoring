 using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class SystemLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; }

        [MaxLength(100)]
        public string Source { get; set; }

        [Required]
        [MaxLength(50)]
        public string Severity { get; set; }

        [Required]
        public string Message { get; set; }

        [MaxLength(100)]
        public string UserId { get; set; }

        [MaxLength(50)]
        public string DeviceId { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }

        public string AdditionalData { get; set; }

        public SystemLog()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}