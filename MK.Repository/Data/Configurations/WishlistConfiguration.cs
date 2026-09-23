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
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.HasOne(x => x.User)
                .WithOne(x => x.Wishlist)
                .HasForeignKey<Wishlist>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
