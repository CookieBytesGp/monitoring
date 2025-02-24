using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Aggregates.Page;

namespace Persistence.Page.Configuration
{
    internal class PageConfiguration : IEntityTypeConfiguration<Domain.Aggregates.Page.Page>
    {
        public void Configure(EntityTypeBuilder<Domain.Aggregates.Page.Page> builder)
        {
            // Configure properties
            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired();

            // Configure Elements as an owned collection
            builder.OwnsMany(p => p.Elements, elementsBuilder =>
            {
                elementsBuilder.Property<int>("Id"); // Shadow property
                elementsBuilder.HasKey("Id");

                elementsBuilder.Property(e => e.ToolId)
                    .IsRequired();

                elementsBuilder.Property(e => e.Order)
                    .IsRequired();

                elementsBuilder.OwnsOne(e => e.TemplateBody, tb =>
                {
                    tb.Property(t => t.HtmlTemplate)
                        .IsRequired();

                    var defaultCssClassesConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Dictionary<string, string>, string>(
                        v => Newtonsoft.Json.JsonConvert.SerializeObject(v),
                        v => string.IsNullOrEmpty(v)
                            ? new Dictionary<string, string>()
                            : Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>()
                    );

                    tb.Property(t => t.DefaultCssClasses)
                        .HasConversion(defaultCssClassesConverter);

                    tb.Property(t => t.CustomCss);
                    tb.Property(t => t.CustomJs);
                    tb.Property(t => t.IsFloating)
                        .IsRequired();
                });

                elementsBuilder.OwnsOne(e => e.Asset, assetBuilder =>
                {
                    assetBuilder.Property(a => a.Url)
                        .IsRequired();
                    assetBuilder.Property(a => a.Type)
                        .IsRequired();
                });
            });
        }
    }
}
