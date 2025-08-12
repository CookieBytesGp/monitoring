using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Aggregates.Page;
using Domain.Aggregates.Page.ValueObjects; // Ensure this using directive is present

namespace Persistence.Page.Configurations
{
    internal class PageConfiguration : IEntityTypeConfiguration<Monitoring.Domain.Aggregates.Page.Page>
    {
        public void Configure(EntityTypeBuilder<Monitoring.Domain.Aggregates.Page.Page> builder)
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
                // Use the existing 'Id' property of 'BaseElement' (type 'Guid')
                elementsBuilder.HasKey(e => e.Id);

                elementsBuilder.Property(e => e.Id)
                    .IsRequired()
                    .ValueGeneratedNever(); // Assuming you set 'Id' in code

                elementsBuilder.Property(e => e.ToolId)
                    .IsRequired();

                elementsBuilder.Property(e => e.Order)
                    .IsRequired();

                // Configure TemplateBody as an owned entity
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
                        .HasConversion(defaultCssClassesConverter)
                        .HasColumnType("TEXT");

                    tb.Property(t => t.CustomCss);
                    tb.Property(t => t.CustomJs);
                    tb.Property(t => t.IsFloating)
                        .IsRequired();
                });

                // Configure Asset as an owned entity
                elementsBuilder.OwnsOne(e => e.Asset, assetBuilder =>
                {
                    assetBuilder.Property(a => a.Url)
                        .IsRequired();

                    assetBuilder.Property(a => a.Type)
                        .IsRequired();

                    // Configure the 'AltText' property if applicable
                    assetBuilder.Property(a => a.AltText);

                    // Configure the 'Content' property if applicable
                    assetBuilder.Property(a => a.Content);

                    // Configure the 'Metadata' property with a Value Converter
                    var metadataConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Dictionary<string, string>, string>(
                        v => Newtonsoft.Json.JsonConvert.SerializeObject(v),
                        v => string.IsNullOrEmpty(v) ? new Dictionary<string, string>()
                            : Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>()
                    );

                    assetBuilder.Property(a => a.Metadata)
                        .HasConversion(metadataConverter)
                        .HasColumnType("TEXT");
                });

            });
        }
    }
}
