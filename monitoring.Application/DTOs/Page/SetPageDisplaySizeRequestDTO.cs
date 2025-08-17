using System.ComponentModel.DataAnnotations;

namespace Monitoring.Application.DTOs.Page
{
    public class SetPageDisplaySizeRequestDTO
    {
        [Required]
        [Range(1, 7680)] // Max 8K width
        public int Width { get; set; }
        
        [Required]
        [Range(1, 4320)] // Max 8K height
        public int Height { get; set; }
        
        public string? Orientation { get; set; } // اختیاری - اگر نباشد محاسبه می‌شود
        
        [Url]
        public string? ThumbnailUrl { get; set; }
    }
}
