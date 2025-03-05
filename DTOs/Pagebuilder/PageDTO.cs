namespace DTOs.Pagebuilder
{
    public class PageDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BaseElementDTO> Elements { get; set; } = new List<BaseElementDTO>();
    }
}
