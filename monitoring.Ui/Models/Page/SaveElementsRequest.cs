using System;
using System.Collections.Generic;

namespace Monitoring.Ui.Models.Page
{
    public class SaveElementsRequest
    {
        public Guid PageId { get; set; }
        public List<PageElementData> Elements { get; set; } = new List<PageElementData>();
    }

    public class PageElementData
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ElementConfig Config { get; set; } = new ElementConfig();
        public ElementPosition Position { get; set; } = new ElementPosition();
        public ElementStyles Styles { get; set; } = new ElementStyles();
        public ElementContent Content { get; set; } = new ElementContent();
        public DateTime Timestamp { get; set; }
    }

    public class ElementConfig
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Content { get; set; } = string.Empty;
        public string FontSize { get; set; } = "14px";
        public string Color { get; set; } = "#000000";
        public string BackgroundColor { get; set; } = "transparent";
        public string Src { get; set; } = string.Empty;
        public string Alt { get; set; } = string.Empty;
        public bool Autoplay { get; set; }
        public bool Loop { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public bool ShowSeconds { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class ElementPosition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ElementStyles
    {
        public string BackgroundColor { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string FontSize { get; set; } = string.Empty;
        public string FontFamily { get; set; } = string.Empty;
        public string FontWeight { get; set; } = string.Empty;
        public string TextAlign { get; set; } = string.Empty;
        public string Border { get; set; } = string.Empty;
        public string BorderRadius { get; set; } = string.Empty;
        public string BoxShadow { get; set; } = string.Empty;
        public string Opacity { get; set; } = string.Empty;
        public string ZIndex { get; set; } = string.Empty;
    }

    public class ElementContent
    {
        public string InnerHTML { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
}
