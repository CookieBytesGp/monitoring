using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monitoring.Domain.Aggregates.Page;
using Domain.Aggregates.Page.ValueObjects;
using Domain.SharedKernel;

namespace Monitoring.Infrastructure.Configuration.Page
{
    internal class PageConfiguration : IEntityTypeConfiguration<Monitoring.Domain.Aggregates.Page.Page>
    {
        public void Configure(EntityTypeBuilder<Monitoring.Domain.Aggregates.Page.Page> builder)
        {
            // Configure table name explicitly
            builder.ToTable("Pages");
            
            // Configure primary properties
            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false); // Make UpdatedAt nullable

            // Configure PageStatus as owned value object
            builder.OwnsOne(p => p.Status, statusBuilder =>
            {
                statusBuilder.Property(s => s.Value)
                    .HasColumnName("Status")
                    .IsRequired();

                statusBuilder.Property(s => s.Name)
                    .HasColumnName("StatusName")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Configure DisplayConfiguration as owned value object
            builder.OwnsOne(p => p.DisplayConfig, displayBuilder =>
            {
                displayBuilder.Property(d => d.Width)
                    .HasColumnName("DisplayWidth")
                    .IsRequired();

                displayBuilder.Property(d => d.Height)
                    .HasColumnName("DisplayHeight")
                    .IsRequired();

                displayBuilder.Property(d => d.ThumbnailUrl)
                    .HasColumnName("ThumbnailUrl")
                    .HasMaxLength(500)
                    .IsRequired(false); // Make nullable since it's set later

                // Configure DisplayOrientation as owned value object
                displayBuilder.OwnsOne(d => d.Orientation, orientationBuilder =>
                {
                    orientationBuilder.Property(o => o.Value)
                        .HasColumnName("DisplayOrientation")
                        .IsRequired();

                    orientationBuilder.Property(o => o.Name)
                        .HasColumnName("DisplayOrientationName")
                        .HasMaxLength(50)
                        .IsRequired();
                });
            });

            // Configure BackgroundAsset as owned value object (nullable)
            builder.OwnsOne(p => p.BackgroundAsset, assetBuilder =>
            {
                assetBuilder.Property(a => a.Url)
                    .HasColumnName("BackgroundAssetUrl")
                    .HasMaxLength(500)
                    .IsRequired(false); // Make nullable

                assetBuilder.Property(a => a.Type)
                    .HasColumnName("BackgroundAssetType")
                    .HasMaxLength(50)
                    .IsRequired(false); // Make nullable

                assetBuilder.Property(a => a.AltText)
                    .HasColumnName("BackgroundAssetAltText")
                    .HasMaxLength(200)
                    .IsRequired(false); // Make nullable

                assetBuilder.Property(a => a.Content)
                    .HasColumnName("BackgroundAssetContent")
                    .HasColumnType("TEXT")
                    .IsRequired(false); // Make nullable

                // Configure Metadata with JSON converter
                var metadataConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Dictionary<string, string>, string>(
                    v => Newtonsoft.Json.JsonConvert.SerializeObject(v),
                    v => string.IsNullOrEmpty(v) ? new Dictionary<string, string>()
                        : Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>()
                );

                assetBuilder.Property(a => a.Metadata)
                    .HasColumnName("BackgroundAssetMetadata")
                    .HasConversion(metadataConverter)
                    .HasColumnType("TEXT")
                    .IsRequired(false); // Make nullable
            });

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

            // Configure Entity base properties to be nullable or have default values
            builder.Property<string>("CreatedBy")
                .HasDefaultValue("System")
                .IsRequired();

            builder.Property<string>("UpdatedBy")
                .HasDefaultValue("System")
                .IsRequired();
        }
    }
}
