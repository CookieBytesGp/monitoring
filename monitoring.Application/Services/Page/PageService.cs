using Domain.Aggregates.Page;
using Domain.Aggregates.Page.ValueObjects;
using Domain.SharedKernel;
using DTOs.Pagebuilder;
using FluentResults;
using Monitoring.Application.DTOs.Page;
using Monitoring.Application.Interfaces.Page;
using Monitoring.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monitoring.Application.Services.Page
{
    public class PageService : IPageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Manual mapping methods
        private PageDTO MapToDto(Monitoring.Domain.Aggregates.Page.Page page)
        {
            if (page == null) return null;

            return new PageDTO
            {
                Id = page.Id,
                Title = page.Title,
                CreatedAt = page.CreatedAt,
                UpdatedAt = page.UpdatedAt,
                Status = page.Status?.Name,
                DisplayConfig = new DisplayConfigurationDTO
                {
                    Width = page.DisplayConfig.Width,
                    Height = page.DisplayConfig.Height,
                    ThumbnailUrl = page.DisplayConfig.ThumbnailUrl,
                    Orientation = page.DisplayConfig.Orientation?.Name,
                    CommonAspectRatio = page.DisplayConfig.AspectRatio.ToString()
                },
                BackgroundAsset = page.BackgroundAsset != null ? MapAssetToDto(page.BackgroundAsset) : null,
                Elements = page.Elements?.Select(MapElementToDto).ToList() ?? new List<BaseElementDTO>()
            };
        }

        private List<PageDTO> MapToDtoList(IEnumerable<Monitoring.Domain.Aggregates.Page.Page> pages)
        {
            return pages?.Select(MapToDto).ToList() ?? new List<PageDTO>();
        }

        private BaseElementDTO MapElementToDto(BaseElement element)
        {
            if (element == null) return null;

            return new BaseElementDTO
            {
                Id = element.Id,
                ToolId = element.ToolId,
                Order = element.Order,
                TemplateBody = element.TemplateBody != null ? MapTemplateBodyToDto(element.TemplateBody) : null,
                Asset = element.Asset != null ? MapAssetToDto(element.Asset) : null
            };
        }

        private AssetDTO MapAssetToDto(Asset asset)
        {
            if (asset == null) return null;

            return new AssetDTO
            {
                Url = asset.Url,
                Type = asset.Type,
                AltText = asset.AltText,
                Content = asset.Content,
                Metadata = asset.Metadata
            };
        }

        private TemplateBodyDTO MapTemplateBodyToDto(TemplateBody templateBody)
        {
            if (templateBody == null) return null;

            return new TemplateBodyDTO
            {
                HtmlTemplate = templateBody.HtmlTemplate,
                DefaultCssClasses = templateBody.DefaultCssClasses,
                CustomCss = templateBody.CustomCss,
                CustomJs = templateBody.CustomJs,
                IsFloating = templateBody.IsFloating
            };
        }

        private BaseElement MapElementFromDto(BaseElementDTO dto)
        {
            if (dto == null) return null;

            // Map TemplateBody and Asset from DTOs
            var templateBody = MapTemplateBodyFromDto(dto.TemplateBody);
            var asset = MapAssetFromDto(dto.Asset);

            // Use the Create method and handle the Result
            var elementResult = BaseElement.Create(dto.ToolId, dto.Order, templateBody, asset);
            
            if (elementResult.IsFailed)
            {
                // Log error or handle failure - for now return null
                return null;
            }

            return elementResult.Value;
        }

        private Asset MapAssetFromDto(AssetDTO dto)
        {
            if (dto == null) return null;

            var assetResult = Asset.Create(
                url: dto.Url,
                type: dto.Type,
                content: dto.Content,
                altText: dto.AltText,
                metadata: dto.Metadata
            );

            return assetResult.IsSuccess ? assetResult.Value : null;
        }

        private TemplateBody MapTemplateBodyFromDto(TemplateBodyDTO dto)
        {
            if (dto == null) return null;

            var templateResult = TemplateBody.Create(
                htmlTemplate: dto.HtmlTemplate,
                defaultCssClasses: dto.DefaultCssClasses ?? new Dictionary<string, string>(),
                customCss: dto.CustomCss,
                customJs: dto.CustomJs,
                isFloating: dto.IsFloating
            );

            return templateResult.IsSuccess ? templateResult.Value : null;
        }

        public async Task<Result<PageDTO>> CreatePageAsync(string title, int displayWidth, int displayHeight, DisplayOrientation orientation, List<BaseElementDTO> elements = null)
        {
            try
            {
                var pageResult = Monitoring.Domain.Aggregates.Page.Page.Create(title, displayWidth, displayHeight, orientation);
                if (pageResult.IsFailed)
                {
                    return Result.Fail<PageDTO>(pageResult.Errors);
                }

                var page = pageResult.Value;

                // Convert and add elements if provided
                if (elements != null && elements.Any())
                {
                    foreach (var elementDto in elements)
                    {
                        var element = MapElementFromDto(elementDto);
                        if (element != null)
                        {
                            page.AddElement(element);
                        }
                    }
                }

                var repository = _unitOfWork.PageRepository;
                await repository.AddAsync(page);
                await _unitOfWork.SaveAsync();

                var pageDto = MapToDto(page);
                return Result.Ok(pageDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<PageDTO>($"Error creating page: {ex.Message}");
            }
        }

        public async Task<Result<PageDTO>> GetPageAsync(Guid id)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(id);
                
                if (page == null)
                {
                    return Result.Fail<PageDTO>("Page not found.");
                }

                var pageDto = MapToDto(page);
                return Result.Ok(pageDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<PageDTO>($"Error getting page: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<PageDTO>>> GetAllAsync()
        {
            try
            {
                var pages = await _unitOfWork.PageRepository.GetAllAsync();
                var pageDTOs = MapToDtoList(pages);
                return Result.Ok(pageDTOs.AsEnumerable());
            }
            catch (Exception ex)
            {
                return Result.Fail<IEnumerable<PageDTO>>($"Error getting all pages: {ex.Message}");
            }
        }

        public async Task<Result> UpdatePageAsync(Guid id, string title, List<BaseElementDTO> elements)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(id);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                // Convert elements using manual mapping
                var domainElements = new List<BaseElement>();
                if (elements != null && elements.Any())
                {
                    foreach (var elementDto in elements)
                    {
                        var element = MapElementFromDto(elementDto);
                        if (element != null)
                        {
                            domainElements.Add(element);
                        }
                    }
                }

                page.Update(title, domainElements);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error updating page: {ex.Message}");
            }
        }

        public async Task<Result> DeletePageAsync(Guid id)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(id);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                await _unitOfWork.PageRepository.RemoveAsync(page);
                await _unitOfWork.SaveAsync();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error deleting page: {ex.Message}");
            }
        }

        public async Task<Result> AddElementAsync(Guid pageId, BaseElementDTO elementDTO)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                var element = MapElementFromDto(elementDTO);
                if (element == null)
                {
                    return Result.Fail("Invalid element data.");
                }

                page.AddElement(element);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error adding element: {ex.Message}");
            }
        }

        public async Task<Result> RemoveElementAsync(Guid pageId, Guid elementId)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.RemoveElementById(elementId);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error removing element: {ex.Message}");
            }
        }

        public async Task<Result> UpdateElementAsync(Guid pageId, Guid elementId, BaseElementDTO elementDTO)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                var element = page.GetElementById(elementId);
                if (element == null)
                {
                    return Result.Fail("Element not found.");
                }

                // Update element using mapped data
                var templateBody = MapTemplateBodyFromDto(elementDTO.TemplateBody);
                var asset = MapAssetFromDto(elementDTO.Asset);

                if (templateBody != null) element.UpdateTemplateBody(templateBody);
                if (asset != null) element.UpdateAsset(asset);
                element.UpdateOrder(elementDTO.Order);

                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error updating element: {ex.Message}");
            }
        }

        // Enhanced Page features
        public async Task<Result> SetPageStatusAsync(Guid pageId, PageStatus status)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.SetStatus(status);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error setting page status: {ex.Message}");
            }
        }

        public async Task<Result> SetPageThumbnailAsync(Guid pageId, string thumbnailUrl)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.SetThumbnail(thumbnailUrl);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error setting page thumbnail: {ex.Message}");
            }
        }

        public async Task<Result> SetPageDisplaySizeAsync(Guid pageId, int width, int height, DisplayOrientation orientation)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.UpdateDisplaySize(width, height, orientation);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error setting page display size: {ex.Message}");
            }
        }

        public async Task<Result> SetPageDisplaySizeAsync(Guid pageId, int width, int height)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.UpdateDisplaySize(width, height);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error setting page display size: {ex.Message}");
            }
        }

        // public async Task<Result> SetBackgroundAssetAsync(Guid pageId, Asset asset)
        // {
        //     try
        //     {
        //         var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
        //         if (page == null)
        //         {
        //             return Result.Fail("Page not found.");
        //         }

        //         page.SetBackgroundAsset(asset);
        //         await _unitOfWork.PageRepository.UpdateAsync(page);
        //         await _unitOfWork.SaveAsync();

        //         return Result.Ok();
        //     }
        //     catch (Exception ex)
        //     {
        //         return Result.Fail($"Error setting background asset: {ex.Message}");
        //     }
        // }

        public async Task<Result> SetBackgroundAssetAsync(Guid pageId, AssetDTO assetDto)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                // Map DTO to domain Asset using factory method
                var assetResult = Asset.Create(
                    url: assetDto.Url,
                    type: assetDto.Type,
                    content: assetDto.Content,
                    altText: assetDto.AltText,
                    metadata: assetDto.Metadata
                );

                if (assetResult.IsFailed)
                {
                    return Result.Fail(assetResult.Errors);
                }

                page.SetBackgroundAsset(assetResult.Value);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error setting background asset: {ex.Message}");
            }
        }

        public async Task<Result> RemoveBackgroundAssetAsync(Guid pageId)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.RemoveBackgroundAsset();
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error removing background asset: {ex.Message}");
            }
        }

        public async Task<Result> ReorderElementsAsync(Guid pageId, List<(Guid elementId, int newOrder)> orderChanges)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.ReorderElements(orderChanges);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error reordering elements: {ex.Message}");
            }
        }
    }
}
