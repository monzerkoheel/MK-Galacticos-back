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
    public class OrderItemTranslationConfiguration : IEntityTypeConfiguration<OrderItemTranslation>
    {
        public void Configure(
            EntityTypeBuilder<OrderItemTranslation> builder)
        {
            builder.ToTable("OrderItemTranslations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Language)
                .HasConversion(new LanguageConverter())
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(x => x.OrderItem)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.OrderItemId,
                x.Language
            })
            .IsUnique();
        }
    }
}
