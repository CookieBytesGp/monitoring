using CBG;
using Domain.Aggregates.Tools;
using Domain.Aggregates.Tools.ValueObjects;
using DTOs.Pagebuilder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Persistence.Tool
{
    public class ToolRepository : Repository<Domain.Aggregates.Tools.Tool>, IToolRepository
    {
        public ToolRepository(DatabaseContext databaseContext) : base(databaseContext: databaseContext)
        {
        }

        public async Task<ToolDTO> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            var result =
                await
                DbSet
                .Where(current => current.Name == name)
                .Select(current => new ToolDTO
                {
                    Id = current.Id,
                    Name = current.Name,
                    DefaultJs = current.DefaultJs,
                    ElementType = current.ElementType,
                    Templates = current.Templates.Select(t => new TemplateDTO
                    {
                        HtmlTemplate = t.HtmlStructure,
                        DefaultCssClasses = t.DefaultCssClasses,
                        CustomCss = t.DefaultCss
                    }).ToList(),
                    DefaultAssets = current.DefaultAssets.Select(a => new AssetDTO
                    {
                        Url = a.Url,
                        Type = a.Type,
                        AltText = a.AltText,
                        Content = a.Content,
                        Metadata = a.Metadata
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            return result;
        }
    }
}
