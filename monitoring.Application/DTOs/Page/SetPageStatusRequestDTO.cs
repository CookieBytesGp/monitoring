using System.ComponentModel.DataAnnotations;

namespace Monitoring.Application.DTOs.Page
{
    public class SetPageStatusRequestDTO
    {
        [Required]
        public string Status { get; set; } // "Draft", "Published", "Archived"
    }
}
