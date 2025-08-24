using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.Application.DTOs.Page
{
    /// <summary>
    /// Request for saving elements from cache to database
    /// </summary>
    public class SaveElementsFromCacheRequest
    {
        [Required]
        public Guid PageId { get; set; }
        
        public string PageTitle { get; set; }
        
        [Required]
        public List<CacheElementData> Elements { get; set; } = new List<CacheElementData>();
    }

    /// <summary>
    /// Cache element data structure from JavaScript
    /// </summary>
    public class CacheElementData
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public CacheElementConfig Config { get; set; } = new CacheElementConfig();
        public CacheElementPosition Position { get; set; } = new CacheElementPosition();
        public CacheElementStyles Styles { get; set; } = new CacheElementStyles();
        public CacheElementContent Content { get; set; } = new CacheElementContent();
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Element configuration from cache
    /// </summary>
    public class CacheElementConfig
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
        public bool? Autoplay { get; set; }
        public bool? Loop { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public bool? ShowSeconds { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    /// <summary>
    /// Element position from cache
    /// </summary>
    public class CacheElementPosition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// Element styles from cache
    /// </summary>
    public class CacheElementStyles
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

    /// <summary>
    /// Element content from cache
    /// </summary>
    public class CacheElementContent
    {
        public string InnerHTML { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
}
