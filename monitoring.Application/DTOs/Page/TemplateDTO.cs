namespace Monitoring.Application.DTOs.Page;

public class TemplateDTO
{
    public string HtmlTemplate { get; set; }
    public Dictionary<string, string> DefaultCssClasses { get; set; }
    public string CustomCss { get; set; }
}
