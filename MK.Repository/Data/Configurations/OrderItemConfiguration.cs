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
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductId)
                .IsRequired(false);

            builder.Property(x => x.ProductImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Gender)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.Size)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.IsCustomPrint)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.PrintName)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.PrintNumber)
                .IsRequired(false);

            builder.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductPrint)
                .WithMany()
                .HasForeignKey(x => x.ProductPrintId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
