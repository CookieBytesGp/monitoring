namespace App.Models.PageEditor
{
    public class RemoveElementRequest
    {
        public Guid PageId { get; set; }
        public Guid ElementId { get; set; }
    }

}
