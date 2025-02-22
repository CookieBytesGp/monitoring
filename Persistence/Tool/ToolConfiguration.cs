using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.SharedKernel;

namespace Persistence.Tool
{
    public class ToolConfiguration : IEntityTypeConfiguration<Domain.Aggregates.Tools.Tool>
    {
        public void Configure(EntityTypeBuilder<Domain.Aggregates.Tools.Tool> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.ElementType)
                .IsRequired();

            builder.OwnsMany(t => t.Templates, templateBuilder =>
            {
                templateBuilder.WithOwner().HasForeignKey("ToolId");
                templateBuilder.Property<Guid>("Id");
                templateBuilder.HasKey("Id");

                templateBuilder.Property(t => t.HtmlStructure).IsRequired();
                templateBuilder.Property(t => t.DefaultCss).IsRequired();

                templateBuilder.Property(t => t.DefaultCssClasses)
                    .HasConversion(
                        v => JsonConvert.SerializeObject(v),
                        v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v))
                    .HasColumnType("TEXT");
            });

            builder.Property(t => t.DefaultAssets)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<Asset>>(v))
                .HasColumnType("TEXT");
        }
    }

}
