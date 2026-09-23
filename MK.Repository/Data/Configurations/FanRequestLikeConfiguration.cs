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
    public class FanRequestLikeConfiguration : IEntityTypeConfiguration<FanRequestLike>
    {
        public void Configure(EntityTypeBuilder<FanRequestLike> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.UserId,
                x.FanRequestId
            })
            .IsUnique();

            builder.HasOne(x => x.User)
                .WithMany(x => x.FanRequestLikes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.FanRequest)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.FanRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
