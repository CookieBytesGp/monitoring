using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Aggregates.User;
using Domain.Aggregates.User.ValueObjects;

namespace Persistence.User.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<Domain.Aggregates.User.User>
    {
        public void Configure(EntityTypeBuilder<Domain.Aggregates.User.User> builder)
        {

            builder
                .Property(p => p.UserName)
                .HasMaxLength(UserName.MaxLength)
                .IsRequired(required: true)
                .HasConversion(p => p.Value,
                p => UserName.Create(p).Value)
                ;

            builder
                .Property(p => p.FirstName)
                .IsRequired(required: true)
                .HasMaxLength(FirstName.MaxLength)
                .UsePropertyAccessMode(propertyAccessMode: PropertyAccessMode.Field)
                .HasColumnName("FirstName")
                .HasConversion(p => p.Value,
                p => FirstName.Create(p).Value)
                ;

            builder
                .Property(p => p.LastName)
                .IsRequired(required: true)
                .HasMaxLength(LastName.MaxLength)
                .UsePropertyAccessMode(propertyAccessMode: PropertyAccessMode.Field)
                .HasColumnName("LastName")
                .HasConversion(p => p.Value,
                p => LastName.Create(p).Value)
                ;

            builder
                .Property(p => p.Password)
                .IsRequired(required: true)
                .HasMaxLength(Password.MaxLength)
                .UsePropertyAccessMode(propertyAccessMode: PropertyAccessMode.Field)
                .HasColumnName("Password")
                .HasConversion(p => p.Value,
                p => Password.Create(p).Value)
                ;



            // تنظیم نام جدول

            // پیکربندی ستون UserName
            //builder
            //    .Property(p => p.UserName)
            //    .HasMaxLength(UserName.MaxLength)
            //    .IsRequired()
            //    .HasConversion(p => p.Value, p => UserName.Create(p).Value);

            //// پیکربندی ستون Password
            //builder
            //    .Property(p => p.Password)
            //    .HasMaxLength(Password.MaxLength)
            //    .IsRequired()
            //    .HasConversion(p => p.Value, p => Password.Create(p).Value);

            //// پیکربندی ویژگی FullName
            //builder.OwnsOne(p => p.FirstName, p =>
            //{
            //    p.Property(pp => pp.Value)
            //        .IsRequired()
            //        .HasColumnName("FirstName")
            //        .HasMaxLength(FirstName.MaxLength)
            //        .HasConversion(pp => pp.Value, pp => FirstName.Create(pp).Value);
            //});

            //builder.OwnsOne(p => p.LastName, p =>
            //{
            //    p.Property(pp => pp.Value)
            //        .IsRequired()
            //        .HasColumnName("LastName")
            //        .HasMaxLength(LastName.MaxLength)
            //        .HasConversion(pp => pp.Value, pp => LastName.Create(pp).Value);
            //});


        }
    }
}
