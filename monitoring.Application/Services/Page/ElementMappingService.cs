using Domain.Aggregates.Page.ValueObjects;
using Domain.SharedKernel;
using DTOs.Pagebuilder;
using FluentResults;
using Monitoring.Application.DTOs.Page;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monitoring.Application.Services.Page
{
    public class ElementMappingService
    {
        private readonly Dictionary<string, Guid> _elementTypeToToolIdMap;
        
        public ElementMappingService()
        {
            // نقشه‌برداری انواع Element به ToolId (باید از دیتابیس یا کانفیگ آمده باشد)
            _elementTypeToToolIdMap = new Dictionary<string, Guid>
            {
                { "text", Guid.Parse("11111111-1111-1111-1111-111111111111") },
                { "image", Guid.Parse("22222222-2222-2222-2222-222222222222") },
                { "video", Guid.Parse("33333333-3333-3333-3333-333333333333") },
                { "camera", Guid.Parse("44444444-4444-4444-4444-444444444444") },
                { "clock", Guid.Parse("55555555-5555-5555-5555-555555555555") },
                { "weather", Guid.Parse("66666666-6666-6666-6666-666666666666") }
            };
        }

        public Result<List<BaseElementDTO>> MapCacheElementsToDomain(List<CacheElementData> cacheElements)
        {
            var domainElements = new List<BaseElementDTO>();
            var errors = new List<string>();

            for (int i = 0; i < cacheElements.Count; i++)
            {
                var cacheElement = cacheElements[i];
                var mappingResult = MapSingleCacheElementToDomain(cacheElement, i);
                
                if (mappingResult.IsSuccess)
                {
                    domainElements.Add(mappingResult.Value);
                }
                else
                {
                    errors.AddRange(mappingResult.Errors.Select(e => e.Message));
                }
            }

            if (errors.Any())
            {
                return Result.Fail<List<BaseElementDTO>>(string.Join("; ", errors));
            }

            return Result.Ok(domainElements);
        }

        public Result<BaseElementDTO> MapSingleCacheElementToDomain(CacheElementData cacheElement, int order)
        {
            try
            {
                // دریافت ToolId بر اساس نوع Element
                if (!_elementTypeToToolIdMap.TryGetValue(cacheElement.Type.ToLower(), out var toolId))
                {
                    return Result.Fail<BaseElementDTO>($"Unknown element type: {cacheElement.Type}");
                }

                // ایجاد TemplateBody از اطلاعات Cache
                var templateBodyResult = CreateTemplateBodyFromCache(cacheElement);
                if (templateBodyResult.IsFailed)
                {
                    return Result.Fail<BaseElementDTO>(templateBodyResult.Errors);
                }

                // ایجاد Asset از اطلاعات Cache
                var assetResult = CreateAssetFromCache(cacheElement);
                if (assetResult.IsFailed)
                {
                    return Result.Fail<BaseElementDTO>(assetResult.Errors);
                }

                // ایجاد BaseElementDTO
                var elementDto = new BaseElementDTO
                {
                    Id = Guid.TryParse(cacheElement.Id, out var parsedId) ? parsedId : Guid.NewGuid(),
                    ToolId = toolId,
                    Order = order,
                    TemplateBody = templateBodyResult.Value,
                    Asset = assetResult.Value
                };

                return Result.Ok(elementDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<BaseElementDTO>($"Error mapping cache element: {ex.Message}");
            }
        }

        private Result<TemplateBodyDTO> CreateTemplateBodyFromCache(CacheElementData cacheElement)
        {
            try
            {
                // ایجاد HTML Template بر اساس نوع Element
                var htmlTemplate = GenerateHtmlTemplate(cacheElement);
                
                // ایجاد CSS Classes پیش‌فرض
                var defaultCssClasses = GenerateDefaultCssClasses(cacheElement);
                
                // ایجاد Custom CSS از styles
                var customCss = GenerateCustomCss(cacheElement);

                var templateBodyDto = new TemplateBodyDTO
                {
                    HtmlTemplate = htmlTemplate,
                    DefaultCssClasses = defaultCssClasses,
                    CustomCss = customCss,
                    CustomJs = "", // JavaScript سفارشی در صورت نیاز
                    IsFloating = false // قابل تنظیم بر اساس نوع Element
                };

                return Result.Ok(templateBodyDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<TemplateBodyDTO>($"Error creating template body: {ex.Message}");
            }
        }

        private Result<AssetDTO> CreateAssetFromCache(CacheElementData cacheElement)
        {
            try
            {
                var assetDto = new AssetDTO();

                switch (cacheElement.Type.ToLower())
                {
                    case "text":
                        assetDto.Type = "text";
                        assetDto.Content = cacheElement.Config.Content ?? cacheElement.Content.TextContent ?? "";
                        assetDto.Url = null;
                        break;

                    case "image":
                        assetDto.Type = "image";
                        assetDto.Url = cacheElement.Config.Src ?? "";
                        assetDto.AltText = cacheElement.Config.Alt ?? "";
                        assetDto.Content = null;
                        break;

                    case "video":
                        assetDto.Type = "video";
                        assetDto.Url = cacheElement.Config.Src ?? "";
                        assetDto.Content = null;
                        assetDto.Metadata = new Dictionary<string, string>
                        {
                            { "autoplay", cacheElement.Config.Autoplay.ToString() },
                            { "loop", cacheElement.Config.Loop.ToString() }
                        };
                        break;

                    case "camera":
                        assetDto.Type = "camera";
                        assetDto.Content = cacheElement.Config.Title ?? "دوربین";
                        assetDto.Url = null;
                        break;

                    case "clock":
                        assetDto.Type = "clock";
                        assetDto.Content = "clock_widget";
                        assetDto.Metadata = new Dictionary<string, string>
                        {
                            { "format", cacheElement.Config.Format ?? "24" },
                            { "showSeconds", cacheElement.Config.ShowSeconds.ToString() }
                        };
                        break;

                    case "weather":
                        assetDto.Type = "weather";
                        assetDto.Content = cacheElement.Config.Location ?? "تهران";
                        assetDto.Metadata = new Dictionary<string, string>
                        {
                            { "location", cacheElement.Config.Location ?? "تهران" }
                        };
                        break;

                    default:
                        assetDto.Type = "unknown";
                        assetDto.Content = cacheElement.Content.TextContent ?? "";
                        break;
                }

                return Result.Ok(assetDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<AssetDTO>($"Error creating asset: {ex.Message}");
            }
        }

        private string GenerateHtmlTemplate(CacheElementData cacheElement)
        {
            return cacheElement.Type.ToLower() switch
            {
                "text" => "<div class=\"text-element\">{{content}}</div>",
                "image" => "<img class=\"image-element\" src=\"{{src}}\" alt=\"{{alt}}\" />",
                "video" => "<video class=\"video-element\" src=\"{{src}}\" controls></video>",
                "camera" => "<div class=\"camera-element\"><i class=\"fas fa-camera\"></i><span>{{title}}</span></div>",
                "clock" => "<div class=\"clock-element\"><div class=\"clock-time\">{{time}}</div></div>",
                "weather" => "<div class=\"weather-element\"><div class=\"weather-info\">{{location}}</div></div>",
                _ => "<div class=\"unknown-element\">{{content}}</div>"
            };
        }

        private Dictionary<string, string> GenerateDefaultCssClasses(CacheElementData cacheElement)
        {
            var classes = new Dictionary<string, string>
            {
                { "container", $"element-{cacheElement.Type}" },
                { "position", "absolute" }
            };

            // افزودن کلاس‌های خاص هر نوع
            switch (cacheElement.Type.ToLower())
            {
                case "text":
                    classes.Add("text", "text-content");
                    break;
                case "image":
                    classes.Add("image", "image-content");
                    break;
                case "video":
                    classes.Add("video", "video-content");
                    break;
            }

            return classes;
        }

        private string GenerateCustomCss(CacheElementData cacheElement)
        {
            var cssRules = new List<string>();

            // اضافه کردن موقعیت و اندازه
            cssRules.Add($"left: {cacheElement.Position.X}px;");
            cssRules.Add($"top: {cacheElement.Position.Y}px;");
            cssRules.Add($"width: {cacheElement.Position.Width}px;");
            cssRules.Add($"height: {cacheElement.Position.Height}px;");

            // اضافه کردن styles از cache
            if (!string.IsNullOrEmpty(cacheElement.Styles.BackgroundColor))
                cssRules.Add($"background-color: {cacheElement.Styles.BackgroundColor};");

            if (!string.IsNullOrEmpty(cacheElement.Styles.Color))
                cssRules.Add($"color: {cacheElement.Styles.Color};");

            if (!string.IsNullOrEmpty(cacheElement.Styles.FontSize))
                cssRules.Add($"font-size: {cacheElement.Styles.FontSize};");

            if (!string.IsNullOrEmpty(cacheElement.Styles.FontFamily))
                cssRules.Add($"font-family: {cacheElement.Styles.FontFamily};");

            if (!string.IsNullOrEmpty(cacheElement.Styles.Border))
                cssRules.Add($"border: {cacheElement.Styles.Border};");

            if (!string.IsNullOrEmpty(cacheElement.Styles.BorderRadius))
                cssRules.Add($"border-radius: {cacheElement.Styles.BorderRadius};");

            return string.Join(" ", cssRules);
        }

        public Dictionary<string, Guid> GetElementTypeToToolIdMap()
        {
            return new Dictionary<string, Guid>(_elementTypeToToolIdMap);
        }

        public void UpdateToolMapping(string elementType, Guid toolId)
        {
            _elementTypeToToolIdMap[elementType.ToLower()] = toolId;
        }
    }
}
