using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.Ui.Models.Page
{
    // ViewModel برای نمایش اطلاعات صفحه در UI
    public class PageViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; }
        public DisplayConfigurationViewModel DisplayConfig { get; set; }
        public AssetViewModel BackgroundAsset { get; set; }
        public List<ElementViewModel> Elements { get; set; } = new List<ElementViewModel>();
        
        // Computed Properties
        public int ElementsCount => Elements?.Count ?? 0;
        public bool HasBackgroundAsset => BackgroundAsset != null;
        public bool IsEmpty => ElementsCount == 0;
    }

    // ViewModel برای پیکربندی نمایش
    public class DisplayConfigurationViewModel
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Orientation { get; set; }
        public string CommonAspectRatio { get; set; }
        
        // Computed Properties
        public double AspectRatio => Height > 0 ? Math.Round((double)Width / Height, 2) : 0;
        public bool IsWidescreen => AspectRatio >= 1.77;
        public bool IsUltraWide => AspectRatio >= 2.35;
        public string Resolution => $"{Width}x{Height}";
        public bool IsPortrait => Orientation?.Equals("Portrait", StringComparison.OrdinalIgnoreCase) == true;
        public bool IsLandscape => Orientation?.Equals("Landscape", StringComparison.OrdinalIgnoreCase) == true;
        public bool IsSquare => Orientation?.Equals("Square", StringComparison.OrdinalIgnoreCase) == true;
    }

    // ViewModel برای المنت‌ها
    public class ElementViewModel
    {
        public Guid Id { get; set; }
        public Guid ToolId { get; set; }
        public int Order { get; set; }
        public TemplateBodyViewModel TemplateBody { get; set; }
        public AssetViewModel Asset { get; set; }
    }

    // ViewModel برای Asset
    public class AssetViewModel
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string AltText { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    // ViewModel برای TemplateBody
    public class TemplateBodyViewModel
    {
        public string HtmlTemplate { get; set; }
        public Dictionary<string, string> DefaultCssClasses { get; set; } = new Dictionary<string, string>();
        public string CustomCss { get; set; }
        public string CustomJs { get; set; }
        public bool IsFloating { get; set; }
    }

    // ViewModel برای ایجاد صفحه جدید
    public class CreatePageViewModel
    {
        [Required(ErrorMessage = "عنوان اجباری است")]
        [StringLength(200, ErrorMessage = "عنوان نباید بیش از 200 کاراکتر باشد")]
        public string Title { get; set; }

        [Required]
        [Range(1, 7680, ErrorMessage = "عرض باید بین 1 تا 7680 پیکسل باشد")]
        public int DisplayWidth { get; set; } = 1920;

        [Required]
        [Range(1, 4320, ErrorMessage = "ارتفاع باید بین 1 تا 4320 پیکسل باشد")]
        public int DisplayHeight { get; set; } = 1080;

        [Required]
        public string Orientation { get; set; } = "Landscape";
    }

    // ViewModel برای ویرایش صفحه
    public class EditPageViewModel : CreatePageViewModel
    {
        public Guid Id { get; set; }
        
        [Url(ErrorMessage = "آدرس تصویر بندانگشتی معتبر نیست")]
        public string? ThumbnailUrl { get; set; } // Made nullable to avoid issues when not provided
        
        public string? Status { get; set; }
        public int ElementsCount { get; set; }
        public bool HasBackgroundAsset { get; set; }
        
        // Tools available for this page editor
        public List<ToolViewModel> AvailableTools { get; set; } = new List<ToolViewModel>();
    }

    // ViewModel برای Tools
    public class ToolViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ElementType { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public TemplateViewModel Template { get; set; }
        public List<AssetViewModel> Assets { get; set; } = new List<AssetViewModel>();
    }

    // ViewModel برای Template
    public class TemplateViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string HtmlTemplate { get; set; }
        public string CustomCss { get; set; }
        public string CustomJs { get; set; }
        public bool IsDefault { get; set; }
        public string PreviewImageUrl { get; set; }
        public string ConfigSchema { get; set; }
    }

    // Request Model برای ارسال به API
    public class CreatePageRequest
    {
        public string Title { get; set; }
        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }
        public string Orientation { get; set; }
        public List<ElementRequest> Elements { get; set; } = new List<ElementRequest>();
    }

    // Request Model برای آپدیت صفحه
    public class UpdatePageRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<ElementRequest> Elements { get; set; } = new List<ElementRequest>();
    }

    // Request Model برای آپدیت اندازه نمایش
    public class UpdateDisplaySizeRequest
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Orientation { get; set; }
    }

    // Request Model برای آپدیت وضعیت
    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }

    // Request Model برای آپدیت تصویر بندانگشتی
    public class UpdateThumbnailRequest
    {
        public string ThumbnailUrl { get; set; }
    }

    // Request Model برای المنت
    public class ElementRequest
    {
        public Guid Id { get; set; }
        public Guid ToolId { get; set; }
        public int Order { get; set; }
        public TemplateBodyRequest TemplateBody { get; set; }
        public AssetRequest Asset { get; set; }
    }

    // Request Model برای TemplateBody
    public class TemplateBodyRequest
    {
        public string HtmlTemplate { get; set; }
        public Dictionary<string, string> DefaultCssClasses { get; set; } = new Dictionary<string, string>();
        public string CustomCss { get; set; }
        public string CustomJs { get; set; }
        public bool IsFloating { get; set; }
    }

    // Request Model برای Asset
    public class AssetRequest
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string AltText { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}