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
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Gender)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.Size)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(x => x.CustomPrintName)
                .HasMaxLength(100);

            builder.Property(x => x.CustomPrintNumber)
                .IsRequired(false);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.IsCustomPrint)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(x => x.Cart)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductPrint)
                .WithMany()
                .HasForeignKey(x => x.ProductPrintId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new
            {
                x.CartId,
                x.ProductId,
                x.Gender,
                x.Size,
                x.ProductPrintId,
                x.CustomPrintName,
                x.CustomPrintNumber
            })
            .IsUnique();
        }
    }
}
