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
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Gender)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.IsMain)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DisplayOrder)
                .IsRequired();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // كل Product + Gender له صورة Main واحدة فقط
            builder.HasIndex(x => new
            {
                x.ProductId,
                x.Gender,
                x.IsMain
            })
            .IsUnique()
            .HasFilter("[IsMain] = 1");
        }
    }
}
