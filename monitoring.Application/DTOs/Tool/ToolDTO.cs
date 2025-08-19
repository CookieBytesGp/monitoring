namespace Monitoring.Application.DTOs.Tool
{
    public class ToolDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ElementType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int Order { get; set; }
        public string ConfigSchema { get; set; } = string.Empty;
        public List<TemplateDTO> Templates { get; set; } = new List<TemplateDTO>();
        public List<AssetDTO> DefaultAssets { get; set; } = new List<AssetDTO>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TemplateDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HtmlTemplate { get; set; } = string.Empty;
        public string CustomCss { get; set; } = string.Empty;
        public string CustomJs { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public string PreviewImageUrl { get; set; } = string.Empty;
        public string ConfigSchema { get; set; } = string.Empty;
    }

    public class AssetDTO
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
