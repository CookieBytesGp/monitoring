using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Aggregates.Page.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Persistence.BaseElemnt
{
    internal class BaseElementConfiguration : IEntityTypeConfiguration<BaseElement>
    {
        public void Configure(EntityTypeBuilder<BaseElement> builder)
        {
            // Configure properties
            builder.Property(b => b.ToolId)
                .IsRequired();

            builder.Property(b => b.Order)
                .IsRequired();

            // Configure ContentConfig as JSON
            var contentConfigConverter = new ValueConverter<Dictionary<string, object>, string>(
                v => JsonConvert.SerializeObject(v),
                v => string.IsNullOrEmpty(v) 
                    ? new Dictionary<string, object>() 
                    : JsonConvert.DeserializeObject<Dictionary<string, object>>(v) ?? new Dictionary<string, object>()
            );

            builder.Property(b => b.ContentConfig)
                .HasConversion(contentConfigConverter)
                .HasColumnType("nvarchar(max)");

            // Configure StyleConfig as JSON
            var styleConfigConverter = new ValueConverter<Dictionary<string, object>, string>(
                v => JsonConvert.SerializeObject(v),
                v => string.IsNullOrEmpty(v) 
                    ? new Dictionary<string, object>() 
                    : JsonConvert.DeserializeObject<Dictionary<string, object>>(v) ?? new Dictionary<string, object>()
            );

            builder.Property(b => b.StyleConfig)
                .HasConversion(styleConfigConverter)
                .HasColumnType("nvarchar(max)");

            builder.OwnsOne(b => b.TemplateBody, tb =>
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

            builder.OwnsOne(b => b.Asset, assetBuilder =>
            {
                assetBuilder.Property(a => a.Url)
                    .IsRequired();
                assetBuilder.Property(a => a.Type)
                    .IsRequired();
            });
        }
    }
}
