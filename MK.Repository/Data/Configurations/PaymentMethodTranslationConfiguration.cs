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
    public class PaymentMethodTranslationConfiguration : IEntityTypeConfiguration<PaymentMethodTranslation>
    {
        public void Configure(
            EntityTypeBuilder<PaymentMethodTranslation> builder)
        {
            builder.ToTable("PaymentMethodTranslations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Language)
                .HasConversion(new LanguageConverter())
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(x => x.PaymentMethod)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.PaymentMethodId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.PaymentMethodId,
                x.Language
            })
            .IsUnique();
        }
    }
}
