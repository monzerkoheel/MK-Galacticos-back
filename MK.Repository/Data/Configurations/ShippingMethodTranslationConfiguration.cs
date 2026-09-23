using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Repository.Data.Configurations
{
    public class ShippingMethodTranslationConfiguration : IEntityTypeConfiguration<ShippingMethodTranslation>
    {
        public void Configure(
            EntityTypeBuilder<ShippingMethodTranslation> builder)
        {
            builder.ToTable("ShippingMethodTranslations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Language)
                .HasConversion(new LanguageConverter())
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(x => x.ShippingMethod)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.ShippingMethodId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.ShippingMethodId,
                x.Language
            })
            .IsUnique();
        }
    }
}
