using Domain.SharedKernel;
using Domain.Aggregates.Page.ValueObjects;
using Monitoring.Application.DTOs.Page;
using System.ComponentModel.DataAnnotations;

namespace DTOs.Pagebuilder
{
    public class PageDTO
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Page Status
        [Required]
        public string Status { get; set; }
        
        // Display Configuration
        [Required]
        public DisplayConfigurationDTO DisplayConfig { get; set; }
        
        // Background Asset (اختیاری)
        public AssetDTO? BackgroundAsset { get; set; }
        
        // Elements Collection
        public List<BaseElementDTO> Elements { get; set; } = new List<BaseElementDTO>();
        
        // Computed Properties
        public int ElementsCount => Elements?.Count ?? 0;
        public bool HasBackgroundAsset => BackgroundAsset != null;
        public bool IsEmpty => ElementsCount == 0;
    }

    public class DisplayConfigurationDTO
    {
        [Required]
        [Range(1, 7680)] // Max 8K width
        public int Width { get; set; }
        
        [Required]
        [Range(1, 4320)] // Max 8K height
        public int Height { get; set; }
        
        [Url]
        public string? ThumbnailUrl { get; set; }
        
        [Required]
        public string Orientation { get; set; }
        
        // Computed Properties
        public double AspectRatio => Height > 0 ? Math.Round((double)Width / Height, 2) : 0;
        public string CommonAspectRatio { get; set; }
        public bool IsWidescreen => AspectRatio >= 1.77;
        public bool IsUltraWide => AspectRatio >= 2.35;
        public string Resolution => $"{Width}x{Height}";
        public bool IsPortrait => Orientation?.Equals("Portrait", StringComparison.OrdinalIgnoreCase) == true;
        public bool IsLandscape => Orientation?.Equals("Landscape", StringComparison.OrdinalIgnoreCase) == true;
        public bool IsSquare => Orientation?.Equals("Square", StringComparison.OrdinalIgnoreCase) == true;
    }
}
