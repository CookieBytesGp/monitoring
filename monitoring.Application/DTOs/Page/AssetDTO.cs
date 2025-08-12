namespace Monitoring.Application.DTOs.Page;

public class AssetDTO
{
    public string Url { get; set; }
    public string Type { get; set; }
    public string AltText { get; set; }
    public string Content { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
