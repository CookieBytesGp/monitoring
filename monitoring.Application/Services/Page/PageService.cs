using AutoMapper;
using Domain.Aggregates.Page;
using Domain.Aggregates.Page.ValueObjects;
using Domain.SharedKernel;
using DTOs.Pagebuilder;
using FluentResults;
using Monitoring.Application.DTOs.Page;
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
        private readonly IMapper _mapper;

        public PageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PageDTO>> CreatePageAsync(string title, int displayWidth, int displayHeight, List<BaseElementDTO> elements = null)
        {
            try
            {
                var pageResult = Monitoring.Domain.Aggregates.Page.Page.Create(title, displayWidth, displayHeight);
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
                        var element = _mapper.Map<BaseElement>(elementDto);
                        if (element != null)
                        {
                            page.AddElement(element);
                        }
                    }
                }

                var repository = _unitOfWork.PageRepository;
                await repository.AddAsync(page);
                await _unitOfWork.SaveAsync();

                var pageDto = _mapper.Map<PageDTO>(page);
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

                var pageDto = _mapper.Map<PageDTO>(page);
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
                var pageDTOs = _mapper.Map<IEnumerable<PageDTO>>(pages);
                return Result.Ok(pageDTOs);
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

                // Convert elements using AutoMapper
                var domainElements = new List<BaseElement>();
                if (elements != null && elements.Any())
                {
                    foreach (var elementDto in elements)
                    {
                        var element = _mapper.Map<BaseElement>(elementDto);
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

                var element = _mapper.Map<BaseElement>(elementDTO);
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
                var templateBody = _mapper.Map<TemplateBody>(elementDTO.TemplateBody);
                var asset = _mapper.Map<Asset>(elementDTO.Asset);

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

        public async Task<Result> SetPageDisplaySizeAsync(Guid pageId, int width, int height)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.SetDisplaySize(width, height);
                await _unitOfWork.PageRepository.UpdateAsync(page);
                await _unitOfWork.SaveAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error setting page display size: {ex.Message}");
            }
        }

        public async Task<Result> SetBackgroundAssetAsync(Guid pageId, Asset asset)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.FindAsync(pageId);
                
                if (page == null)
                {
                    return Result.Fail("Page not found.");
                }

                page.SetBackgroundAsset(asset);
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
