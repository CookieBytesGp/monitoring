using DTOs.Pagebuilder;

namespace App.Models.PageEditor
{
    public class EditorMainViewModel
    {
        public Guid PageId { get; set; }
        public List<BaseElementDTO> Elements { get; set; }
    }
}
