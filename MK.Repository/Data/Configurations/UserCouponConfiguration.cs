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
    public class UserCouponConfiguration : IEntityTypeConfiguration<UserCoupon>
    {
        public void Configure(EntityTypeBuilder<UserCoupon> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.AcquisitionType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.AssignedAt)
                .IsRequired();

            builder.Property(x => x.ExpirationDate)
                .IsRequired();

            builder.Property(x => x.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.UsedAt)
                .IsRequired(false);

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserCoupons)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Coupon)
                .WithMany(x => x.UserCoupons)
                .HasForeignKey(x => x.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                .WithOne(x => x.UserCoupon)
                .HasForeignKey<Order>(x => x.UserCouponId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
