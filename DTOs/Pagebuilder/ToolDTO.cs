namespace DTOs.Pagebuilder
{
    public class ToolDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DefaultJs { get; set; }
        public string ElementType { get; set; }
        public List<TemplateDTO> Templates { get; set; }
        public List<AssetDTO> DefaultAssets { get; set; }
    }
}
