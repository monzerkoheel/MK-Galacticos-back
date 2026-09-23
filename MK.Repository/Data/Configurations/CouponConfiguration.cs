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
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CouponType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.DiscountAmount)
                .IsRequired(false)
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountPercentage)
                .IsRequired(false)
                .HasPrecision(5, 2);

            builder.Property(x => x.RequiredPoints)
                .IsRequired(false);

            builder.Property(x => x.MinimumOrderAmount)
                .IsRequired(false)
                .HasPrecision(18, 2);

            builder.Property(x => x.MaximumDiscountAmount)
                .IsRequired(false)
                .HasPrecision(18, 2);

            builder.Property(x => x.UserVisibility)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.HasMany(x => x.UserCoupons)
                .WithOne(x => x.Coupon)
                .HasForeignKey(x => x.CouponId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
