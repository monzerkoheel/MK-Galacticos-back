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
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(250);

            builder.HasIndex(x => x.Slug)
                .IsUnique();

            builder.Property(x => x.SKU)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.SKU)
                .IsUnique();

            builder.Property(x => x.ProductType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.AvailableFor)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.SalePrice)
                .IsRequired(false)
                .HasPrecision(18, 2);

            builder.Property(x => x.LowStockThreshold)
                .IsRequired()
                .HasDefaultValue(5);

            builder.Property(x => x.Visibility)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsNewArrival)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.IsCustomPrintAvailable)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsOutOfStock)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.AverageRating)
                .IsRequired()
                .HasPrecision(3, 2)
                .HasDefaultValue(0);

            builder.Property(x => x.RatingCount)
                .IsRequired()
                .HasDefaultValue(0);
        }
    }
}
