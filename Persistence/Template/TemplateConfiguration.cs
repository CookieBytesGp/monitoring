//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
//using Newtonsoft.Json;
//using Domain.Aggregates.Tools.ValueObjects; // Adjust this to match your actual namespace

//namespace Persistence.Tools.Configurations
//{
//    internal class TemplateConfiguration : IEntityTypeConfiguration<Template>
//    {
//        public void Configure(EntityTypeBuilder<Template> builder)
//        {
//            builder.HasKey(t => t.Id);

//            builder.Property(t => t.ToolId)
//                .IsRequired();

//            builder.Property(t => t.HtmlStructure)
//                .IsRequired();

//            builder.Property(t => t.DefaultCss)
//                .IsRequired();

//            // Adjusted conversion to handle possible null reference
//            builder.Property(t => t.DefaultCssClasses)
//                .HasConversion(new ValueConverter<Dictionary<string, string>, string>(
//                    v => JsonConvert.SerializeObject(v),
//                    v => string.IsNullOrEmpty(v)
//                        ? new Dictionary<string, string>()
//                        : JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>()));
//        }
//    }
//}
