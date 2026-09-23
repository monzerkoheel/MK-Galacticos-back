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
    public class DeletedAccountConfiguration : IEntityTypeConfiguration<DeletedAccount>
    {
        public void Configure(EntityTypeBuilder<DeletedAccount> builder)
        {
            builder.ToTable("DeletedAccounts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.DeletedAt)
                .IsRequired();

            builder.Property(x => x.DeleteDate)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<DeletedAccount>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.HasIndex(x => x.DeleteDate);
        }
    }
}
