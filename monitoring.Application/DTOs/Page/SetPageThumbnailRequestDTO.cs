using System.ComponentModel.DataAnnotations;

namespace Monitoring.Application.DTOs.Page
{
    public class SetPageThumbnailRequestDTO
    {
        [Required]
        [Url]
        public string ThumbnailUrl { get; set; }
    }
}
