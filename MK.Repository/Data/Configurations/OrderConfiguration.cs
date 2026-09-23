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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);

            // Order Number

            builder.Property(x => x.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.OrderNumber)
                .IsUnique();


            // Status

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);


            // Amounts

            builder.Property(x => x.Subtotal)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.ShippingCost)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);


            // Coupon

            builder.Property(x => x.HasCoupon)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CouponCode)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.CouponDiscountAmount)
                .HasPrecision(18, 2);

            // Gift Card

            builder.Property(x => x.HasLegendCard)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.LegendCardPrice)
                .IsRequired(false)
                .HasPrecision(18, 2);


            // Dates

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();


            // User

            builder.HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // User Coupon

            builder.HasOne(x => x.UserCoupon)
                .WithOne(x => x.Order)
                .HasForeignKey<Order>(x => x.UserCouponId)
                .OnDelete(DeleteBehavior.Restrict);


            // Legend Card

            builder.HasOne(x => x.LegendCard)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.LegendCardId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
