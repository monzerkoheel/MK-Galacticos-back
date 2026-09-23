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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.ProfileImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.LoyaltyPointsBalance)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.LifetimeEarnedLoyaltyPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.LastLoginAt)
                .IsRequired(false);

            builder.Property(x => x.RefreshToken)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.HasIndex(x => x.RefreshToken)
                .IsUnique();

            builder.Property(x => x.RefreshTokenExpiryTime)
                .IsRequired(false);
        }
    }
}
