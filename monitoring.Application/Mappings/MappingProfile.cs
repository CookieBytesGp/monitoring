using AutoMapper;
using Monitoring.Domain.Aggregates.Camera.Entities;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Domain.Aggregates.Page.ValueObjects;
using Domain.Aggregates.Tools.ValueObjects;
using Domain.SharedKernel;
using DTOs.Pagebuilder;
using Monitoring.Application.DTOs.Page;
using Domain.Aggregates.Camera.Entities;
using Domain.Aggregates.Camera.ValueObjects;

namespace Monitoring.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Camera Aggregate Mappings
            CreateCameraMappings();
            
            // Page Aggregate Mappings  
            CreatePageMappings();
            
            // Tools Aggregate Mappings
            CreateToolsMappings();
            
            // Common/Shared Mappings
            CreateSharedMappings();
        }

        private void CreateCameraMappings()
        {
            // Camera Domain to DTO
            CreateMap<Monitoring.Domain.Aggregates.Camera.Camera, Monitoring.Application.DTOs.Camera.CameraDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location.Value))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.Network.IpAddress))
                .ForMember(dest => dest.Port, opt => opt.MapFrom(src => src.Network.Port))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Network.Username))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Network.Password))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name));

            // Camera DTO to Domain - simple mapping without factory (will be improved later)
            CreateMap<Monitoring.Application.DTOs.Camera.CameraDto, Monitoring.Domain.Aggregates.Camera.Camera>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore())
                .ForMember(dest => dest.Network, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            // CreateCameraDto to CameraDto mapping
            CreateMap<Monitoring.Application.DTOs.Camera.CreateCameraDto, Monitoring.Application.DTOs.Camera.CameraDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Disconnected"))
                .ForMember(dest => dest.Zone, opt => opt.MapFrom(src => new Monitoring.Application.DTOs.Camera.ZoneDto 
                { 
                    Id = Guid.NewGuid(), 
                    Name = src.Zone ?? "Default Zone", 
                    Description = "Auto-created zone" 
                }))
                .ForMember(dest => dest.Configuration, opt => opt.MapFrom(src => new Monitoring.Application.DTOs.Camera.CameraConfigurationDto
                {
                    MotionDetection = new Monitoring.Application.DTOs.Camera.MotionDetectionSettingsDto
                    {
                        IsEnabled = false,
                        Sensitivity = 50,
                        DetectionZones = new List<string>()
                    },
                    Recording = new Monitoring.Application.DTOs.Camera.RecordingSettingsDto
                    {
                        IsEnabled = false,
                        Quality = "Medium",
                        StoragePath = "",
                        RetentionDays = 30
                    },
                    Alerts = new Dictionary<string, string>(),
                    Display = new Dictionary<string, string>(),
                    Advanced = new Dictionary<string, string>()
                }))
                .ForMember(dest => dest.Streams, opt => opt.MapFrom(src => new List<Monitoring.Application.DTOs.Camera.CameraStreamDto>()))
                .ForMember(dest => dest.Capabilities, opt => opt.MapFrom(src => new List<Monitoring.Application.DTOs.Camera.CameraCapabilityDto>()));

            // Camera Streams and Capabilities
            CreateMap<CameraStream, Monitoring.Application.DTOs.Camera.CameraStreamDto>();
            CreateMap<CameraCapability, Monitoring.Application.DTOs.Camera.CameraCapabilityDto>();
            CreateMap<CameraConfiguration, Monitoring.Application.DTOs.Camera.CameraConfigurationDto>();
        }

        private void CreatePageMappings()
        {
            // Page Domain to DTO
            CreateMap<Monitoring.Domain.Aggregates.Page.Page, PageDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : "Draft"))
                .ForMember(dest => dest.BackgroundAsset, opt => opt.MapFrom(src => src.BackgroundAsset));

            CreateMap<BaseElement, BaseElementDTO>();
            CreateMap<TemplateBody, TemplateBodyDTO>();

            // Page DTO to Domain - using factory methods
            CreateMap<PageDTO, Monitoring.Domain.Aggregates.Page.Page>()
                .ConstructUsing((src, context) =>
                {
                    var pageResult = Domain.Aggregates.Page.Page.Create(src.Title, src.DisplayWidth, src.DisplayHeight);
                    return pageResult.IsSuccess ? pageResult.Value : null;
                })
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Elements, opt => opt.Ignore());

            CreateMap<BaseElementDTO,BaseElement>()
                .ConstructUsing((src, context) =>
                {
                    var templateBodyResult = TemplateBody.Create(
                        src.TemplateBody?.HtmlTemplate ?? "",
                        src.TemplateBody?.DefaultCssClasses ?? new Dictionary<string, string>(),
                        src.TemplateBody?.CustomCss ?? "",
                        src.TemplateBody?.CustomJs ?? "",
                        src.TemplateBody?.IsFloating ?? false);

                    var assetResult = Asset.Create(
                        src.Asset?.Url ?? "",
                        src.Asset?.Type ?? "text",
                        src.Asset?.Content ?? "",
                        src.Asset?.AltText ?? "",
                        src.Asset?.Metadata ?? new Dictionary<string, string>());

                    if (templateBodyResult.IsSuccess && assetResult.IsSuccess)
                    {
                        var elementResult = BaseElement.Create(
                            src.ToolId,
                            src.Order,
                            templateBodyResult.Value,
                            assetResult.Value);
                        return elementResult.IsSuccess ? elementResult.Value : null;
                    }
                    return null;
                });

            CreateMap<TemplateBodyDTO, TemplateBody>()
                .ConstructUsing((src, context) =>
                {
                    var result = TemplateBody.Create(
                        src.HtmlTemplate ?? "",
                        src.DefaultCssClasses ?? new Dictionary<string, string>(),
                        src.CustomCss ?? "",
                        src.CustomJs ?? "",
                        src.IsFloating);
                    return result.IsSuccess ? result.Value : null;
                });
        }

        private void CreateToolsMappings()
        {
            // Tools Domain to DTO
            CreateMap<Monitoring.Domain.Aggregates.Tools.Tool, ToolDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DefaultJs, opt => opt.MapFrom(src => src.DefaultJs))
                .ForMember(dest => dest.ElementType, opt => opt.MapFrom(src => src.ElementType))
                .ForMember(dest => dest.Templates, opt => opt.MapFrom(src => src.Templates))
                .ForMember(dest => dest.DefaultAssets, opt => opt.MapFrom(src => src.DefaultAssets));

            CreateMap<Template, TemplateDTO>()
                .ForMember(dest => dest.HtmlTemplate, opt => opt.MapFrom(src => src.HtmlStructure))
                .ForMember(dest => dest.DefaultCssClasses, opt => opt.MapFrom(src => src.DefaultCssClasses))
                .ForMember(dest => dest.CustomCss, opt => opt.MapFrom(src => src.DefaultCss));

            // Tools DTO to Domain - using factory methods
            CreateMap<ToolDTO, Monitoring.Domain.Aggregates.Tools.Tool>()
                .ConstructUsing((src, context) =>
                {
                    var templates = src.Templates?.Select(t => 
                    {
                        var templateResult = Template.Create(
                            t.HtmlTemplate ?? "",
                            t.DefaultCssClasses ?? new Dictionary<string, string>(),
                            t.CustomCss ?? "");
                        return templateResult.IsSuccess ? templateResult.Value : null;
                    }).Where(t => t != null).ToList();

                    if (templates != null && templates.Any())
                    {
                        var assets = src.DefaultAssets?.Select(a =>
                        {
                            var assetResult = Asset.Create(
                                a.Url ?? "",
                                a.Type ?? "text",
                                a.Content ?? "",
                                a.AltText ?? "",
                                a.Metadata ?? new Dictionary<string, string>());
                            return assetResult.IsSuccess ? assetResult.Value : null;
                        }).Where(a => a != null).ToList() ?? new List<Asset>();

                        var toolsResult = Domain.Aggregates.Tools.Tool.Create(
                            src.Name,
                            src.DefaultJs,
                            src.ElementType,
                            templates,
                            assets);
                        return toolsResult.IsSuccess ? toolsResult.Value : null;
                    }
                    return null;
                });

            CreateMap<TemplateDTO, Template>()
                .ConstructUsing((src, context) =>
                {
                    var result = Template.Create(
                        src.HtmlTemplate ?? "",
                        src.DefaultCssClasses ?? new Dictionary<string, string>(),
                        src.CustomCss ?? "");
                    return result.IsSuccess ? result.Value : null;
                });
        }

        private void CreateSharedMappings()
        {
            // Asset mappings
            CreateMap<Asset, AssetDTO>();
            CreateMap<AssetDTO, Asset>()
                .ConstructUsing((src, context) =>
                {
                    var result = Asset.Create(
                        src.Url ?? "",
                        src.Type ?? "text",
                        src.Content ?? "",
                        src.AltText ?? "",
                        src.Metadata ?? new Dictionary<string, string>());
                    return result.IsSuccess ? result.Value : null;
                });

            // Common Value Objects mappings
            CreateMap<CameraLocation, string>()
                .ConvertUsing(src => src.Value);
        }
    }
}
