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
    public class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
    {
        public void Configure(EntityTypeBuilder<ProductStock> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Size)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(x => x.StockType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(x => x.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.HasIndex(x => new
            {
                x.ProductId,
                x.Size,
                x.StockType
            })
            .IsUnique();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
