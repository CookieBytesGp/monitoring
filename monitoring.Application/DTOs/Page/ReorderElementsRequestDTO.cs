using System.ComponentModel.DataAnnotations;

namespace Monitoring.Application.DTOs.Page
{
    public class ReorderElementsRequestDTO
    {
        [Required]
        public List<ElementOrderChangeDTO> OrderChanges { get; set; }
    }
    
    public class ElementOrderChangeDTO
    {
        [Required]
        public Guid ElementId { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int NewOrder { get; set; }
    }
}
