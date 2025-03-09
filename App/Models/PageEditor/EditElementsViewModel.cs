using DTOs.Pagebuilder;

namespace App.Models.PageEditor
{
    // ViewModel for the EditElements view
    public class EditElementsViewModel
    {
        public Guid PageId { get; set; }
        public List<ToolDTO> Tools { get; set; }
        public List<BaseElementDTO> Elements { get; set; }
    }
}
