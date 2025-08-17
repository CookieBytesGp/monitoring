using System.ComponentModel.DataAnnotations;
using Monitoring.Application.DTOs.Page;

namespace Monitoring.Application.DTOs.Page
{
    public class CreatePageRequestDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }
        
        [Required]
        [Range(1, 7680)] // Max 8K width
        public int DisplayWidth { get; set; }
        
        [Required]
        [Range(1, 4320)] // Max 8K height
        public int DisplayHeight { get; set; }
        
        [Required]
        public string Orientation { get; set; } // "Portrait", "Landscape", "Square"
        
        [Url]
        public string? ThumbnailUrl { get; set; }
        
        public List<BaseElementDTO>? Elements { get; set; }
        
        public AssetDTO? BackgroundAsset { get; set; }
    }
}
