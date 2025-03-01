namespace DTOs.Pagebuilder
{
    public class TemplateBodyDTO
    {
        public string HtmlTemplate { get; set; }
        public Dictionary<string, string> DefaultCssClasses { get; set; }
        public string CustomCss { get; set; }
        public string CustomJs { get; set; }
        public bool IsFloating { get; set; }
    }
}
