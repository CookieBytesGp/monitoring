using DTOs.Pagebuilder;

namespace App.Models.PageEditor
{
    public class AddToolRequest
    {
        public Guid PageId { get; set; }
        public ToolDTO Tool { get; set; }
    }

}
