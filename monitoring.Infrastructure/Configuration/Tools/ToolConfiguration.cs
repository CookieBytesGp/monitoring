using Domain.Aggregates.Tools;
using Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Monitoring.Infrastructure.Configuration.Tools
{
    internal class ToolConfiguration : IEntityTypeConfiguration<Monitoring.Domain.Aggregates.Tools.Tool>
    {
        public void Configure(EntityTypeBuilder<Monitoring.Domain.Aggregates.Tools.Tool> builder)
        {
            // Configure properties
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.DefaultJs);

            builder.Property(t => t.ElementType)
                .IsRequired()
                .HasMaxLength(50);

            // Configure DefaultAssets property
            var defaultAssetsConverter = new ValueConverter<List<Asset>, string>(
                v => JsonConvert.SerializeObject(v),
                v => string.IsNullOrEmpty(v)
                    ? new List<Asset>()
                    : JsonConvert.DeserializeObject<List<Asset>>(v) ?? new List<Asset>()
            );

            builder.Property(t => t.DefaultAssets)
                .HasConversion(defaultAssetsConverter);

            // Configure Templates as an owned collection without Id
            builder.OwnsMany(t => t.Templates, templatesBuilder =>
            {
                templatesBuilder.WithOwner().HasForeignKey("ToolId");
                templatesBuilder.Property<int>("Id"); // Shadow property for EF Core tracking
                templatesBuilder.HasKey("Id");

                templatesBuilder.Property(t => t.HtmlStructure)
                    .IsRequired();

                templatesBuilder.Property(t => t.DefaultCss)
                    .IsRequired();

                var defaultCssClassesConverter = new ValueConverter<Dictionary<string, string>, string>(
                    v => JsonConvert.SerializeObject(v),
                    v => string.IsNullOrEmpty(v)
                        ? new Dictionary<string, string>()
                        : JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>()
                );

                templatesBuilder.Property(t => t.DefaultCssClasses)
                    .HasConversion(defaultCssClassesConverter);
            });
        }
    }
}
