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
    public class BannedUserConfiguration : IEntityTypeConfiguration<BannedUser>
    {
        public void Configure(EntityTypeBuilder<BannedUser> builder)
        {
            builder.ToTable("BannedUsers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.BannedAt)
                .IsRequired();

            builder.Property(x => x.BannedUntil)
                .IsRequired(false);

            builder.Property(x => x.BanReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<BannedUser>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.HasIndex(x => x.BannedUntil);
        }
    }
}
