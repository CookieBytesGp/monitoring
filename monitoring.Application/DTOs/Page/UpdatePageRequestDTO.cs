using System.ComponentModel.DataAnnotations;
using Monitoring.Application.DTOs.Page;

namespace Monitoring.Application.DTOs.Page
{
    public class UpdatePageRequestDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }
        
        public List<BaseElementDTO>? Elements { get; set; }
        
        public AssetDTO? BackgroundAsset { get; set; }
    }
}
