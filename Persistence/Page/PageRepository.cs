using CBG;
using Domain.Aggregates.Page;
using DTOs.Pagebuilder;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace Persistence.Page
{
    public class PageRepository : Repository<Domain.Aggregates.Page.Page>, IPageRepository
    {
        public PageRepository(DatabaseContext databaseContext) : base(databaseContext)
        {
        }

        public async Task<PageDTO> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
        {
            var result = await DbSet
                .Where(page => page.Title == title)
                .Select(page => new PageDTO
                {
                    Id = page.Id,
                    Title = page.Title,
                    CreatedAt = page.CreatedAt,
                    UpdatedAt = page.UpdatedAt,
                    Elements = page.Elements.Select(element => new BaseElementDTO
                    {
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
                })
                .FirstOrDefaultAsync(cancellationToken);

            return result;
        }
    }
}
