using Domain.Aggregates.Page;
using Domain.Aggregates.Page.ValueObjects;
using Domain.SharedKernel.Domain.SharedKernel;
using DTOs.Pagebuilder;
using FluentResults;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PageBuilder.Services.PageService
{
    public class PageService : IPageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PageDTO>> CreatePageAsync(string title, List<BaseElementDTO> elements)
        {
            var pageResult = Page.Create(title);
            if (pageResult.IsFailed)
            {
                return Result.Fail<PageDTO>(pageResult.Errors);
            }

            var page = pageResult.Value;

            // Convert and add elements if provided.
            if (elements != null && elements.Any())
            {
                var elementConversion = ConvertToDomainElements(elements);
                if (elementConversion.IsFailed)
                {
                    return Result.Fail<PageDTO>(elementConversion.Errors);
                }
                // Update the page title and add elements
                page.Update(title, elementConversion.Value);
            }

            await _unitOfWork.PageRepository.AddAsync(page);
            await _unitOfWork.SaveAsync();

            var pageDTO = ConvertToDTO(page);
            return Result.Ok(pageDTO);
        }

        public async Task<PageDTO> GetPageAsync(Guid id)
        {
            var page = await _unitOfWork.PageRepository.FindAsync(id);
            return page != null ? ConvertToDTO(page) : null;
        }

        public async Task<Result<IEnumerable<PageDTO>>> GetAllAsync()
        {
            var pages = await _unitOfWork.PageRepository.GetAllAsync();
            var pageDTOs = pages.Select(ConvertToDTO).ToList();
            return Result.Ok((IEnumerable<PageDTO>)pageDTOs);
        }

        public async Task<Result> UpdatePageAsync(Guid id, string title, List<BaseElementDTO> elements)
        {
            var page = await _unitOfWork.PageRepository.FindAsync(id);
            if (page == null)
            {
                return Result.Fail("Page not found.");
            }

            // Convert the DTO list to domain elements
            var elementConversion = ConvertToDomainElements(elements);
            if (elementConversion.IsFailed)
            {
                return Result.Fail(elementConversion.Errors);
            }

            page.Update(title, elementConversion.Value);
            await _unitOfWork.PageRepository.UpdateAsync(page);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> DeletePageAsync(Guid id)
        {
            var success = await _unitOfWork.PageRepository.RemoveByIdAsync(id);
            if (!success)
            {
                return Result.Fail("Page not found.");
            }
            await _unitOfWork.SaveAsync();
            return Result.Ok();
        }

        public async Task<Result> AddElementAsync(Guid pageId, BaseElementDTO elementDTO)
        {
            var page = await _unitOfWork.PageRepository.FindAsync(pageId);
            if (page == null)
            {
                return Result.Fail("Page not found.");
            }

            var elementResult = CreateBaseElementFromDTO(elementDTO);
            if (elementResult.IsFailed)
            {
                return Result.Fail(elementResult.Errors);
            }

            page.AddElement(elementResult.Value);
            await _unitOfWork.PageRepository.UpdateAsync(page);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> RemoveElementAsync(Guid pageId, Guid elementId)
        {
            var page = await _unitOfWork.PageRepository.FindAsync(pageId);
            if (page == null)
            {
                return Result.Fail("Page not found.");
            }

            var element = page.Elements.FirstOrDefault(e => e.Id == elementId);
            if (element == null)
            {
                return Result.Fail("Element not found.");
            }

            page.RemoveElement(element);
            await _unitOfWork.PageRepository.UpdateAsync(page);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> UpdateElementAsync(Guid pageId, Guid elementId, BaseElementDTO elementDTO)
        {
            var page = await _unitOfWork.PageRepository.FindAsync(pageId);
            if (page == null)
            {
                return Result.Fail("Page not found.");
            }

            var element = page.Elements.FirstOrDefault(e => e.Id == elementId);
            if (element == null)
            {
                return Result.Fail("Element not found.");
            }

            var templateBodyResult = TemplateBody.Create(
                elementDTO.TemplateBody.HtmlTemplate,
                elementDTO.TemplateBody.DefaultCssClasses,
                elementDTO.TemplateBody.CustomCss,
                elementDTO.TemplateBody.CustomJs,
                elementDTO.TemplateBody.IsFloating);
            if (templateBodyResult.IsFailed)
            {
                return Result.Fail(templateBodyResult.Errors);
            }

            var assetResult = Asset.Create(
                elementDTO.Asset.Url,
                elementDTO.Asset.Type,
                elementDTO.Asset.Content,
                elementDTO.Asset.AltText,
                elementDTO.Asset.Metadata);
            if (assetResult.IsFailed)
            {
                return Result.Fail(assetResult.Errors);
            }

            element.UpdateTemplateBody(templateBodyResult.Value);
            element.UpdateAsset(assetResult.Value);
            element.UpdateOrder(elementDTO.Order);

            await _unitOfWork.PageRepository.UpdateAsync(page);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }

        #region Helper Methods

        private Result<List<BaseElement>> ConvertToDomainElements(List<BaseElementDTO> elementDTOs)
        {
            var elementResults = elementDTOs.Select(dto =>
            {
                var templateBodyResult = TemplateBody.Create(
                    dto.TemplateBody.HtmlTemplate,
                    dto.TemplateBody.DefaultCssClasses,
                    dto.TemplateBody.CustomCss,
                    dto.TemplateBody.CustomJs,
                    dto.TemplateBody.IsFloating);
                var assetResult = Asset.Create(
                    dto.Asset.Url,
                    dto.Asset.Type,
                    dto.Asset.Content,
                    dto.Asset.AltText,
                    dto.Asset.Metadata);
                if (templateBodyResult.IsFailed || assetResult.IsFailed)
                {
                    return Result.Fail<BaseElement>(templateBodyResult.Errors.Concat(assetResult.Errors).ToList());
                }
                if (dto.Id != Guid.Empty)
                {
                    return BaseElement.Rehydrate(
                        dto.Id,
                        dto.ToolId,
                        dto.Order,
                        templateBodyResult.Value,
                        assetResult.Value);
                }
                else
                {
                    return BaseElement.Create(
                        dto.ToolId,
                        dto.Order,
                        templateBodyResult.Value,
                        assetResult.Value);
                }
            }).ToList();

            var failedResults = elementResults.Where(r => r.IsFailed).SelectMany(r => r.Errors).ToList();
            if (failedResults.Any())
            {
                return Result.Fail<List<BaseElement>>(failedResults);
            }

            var elements = elementResults.Select(r => r.Value).ToList();
            return Result.Ok(elements);
        }


        private BaseElementDTO ConvertToBaseElementDTO(BaseElement element)
        {
            return new BaseElementDTO
            {
                Id = element.Id,
                ToolId = element.ToolId,
                Order = element.Order,
                TemplateBody = new TemplateBodyDTO
                {
                    HtmlTemplate = element.TemplateBody.HtmlTemplate,
                    DefaultCssClasses = element.TemplateBody.DefaultCssClasses,
                    CustomCss = element.TemplateBody.CustomCss,
                    CustomJs = element.TemplateBody.CustomJs,
                    IsFloating = element.TemplateBody.IsFloating
                },
                Asset = new AssetDTO
                {
                    Url = element.Asset.Url,
                    Type = element.Asset.Type,
                    Content = element.Asset.Content,
                    AltText = element.Asset.AltText,
                    Metadata = element.Asset.Metadata
                }
            };
        }

        private PageDTO ConvertToDTO(Page page)
        {
            return new PageDTO
            {
                Id = page.Id,
                Title = page.Title,
                CreatedAt = page.CreatedAt,
                UpdatedAt = page.UpdatedAt,
                Elements = (page.Elements ?? new List<BaseElement>())
                            .Select(element => new BaseElementDTO
                            {
                                Id = element.Id,
                                ToolId = element.ToolId,
                                Order = element.Order,
                                TemplateBody = new TemplateBodyDTO
                                {
                                    HtmlTemplate = element.TemplateBody.HtmlTemplate,
                                    DefaultCssClasses = element.TemplateBody.DefaultCssClasses,
                                    CustomCss = element.TemplateBody.CustomCss,
                                    CustomJs = element.TemplateBody.CustomJs,
                                    IsFloating = element.TemplateBody.IsFloating
                                },
                                Asset = new AssetDTO
                                {
                                    Url = element.Asset.Url,
                                    Type = element.Asset.Type,
                                    Content = element.Asset.Content,
                                    AltText = element.Asset.AltText,
                                    Metadata = element.Asset.Metadata
                                }
                            }).ToList()
            };
        }


        private Result<BaseElement> CreateBaseElementFromDTO(BaseElementDTO dto)
        {
            var templateBodyResult = TemplateBody.Create(
                dto.TemplateBody.HtmlTemplate,
                dto.TemplateBody.DefaultCssClasses,
                dto.TemplateBody.CustomCss,
                dto.TemplateBody.CustomJs,
                dto.TemplateBody.IsFloating);

            if (templateBodyResult.IsFailed)
            {
                return Result.Fail<BaseElement>(templateBodyResult.Errors);
            }

            var assetResult = Asset.Create(
                dto.Asset.Url,
                dto.Asset.Type,
                dto.Asset.Content,
                dto.Asset.AltText,
                dto.Asset.Metadata);

            if (assetResult.IsFailed)
            {
                return Result.Fail<BaseElement>(assetResult.Errors);
            }

            return BaseElement.Create(
                dto.ToolId,
                dto.Order,
                templateBodyResult.Value,
                assetResult.Value);
        }

        #endregion
    }
}
