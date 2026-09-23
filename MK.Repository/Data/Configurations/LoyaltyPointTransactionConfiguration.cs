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
    public class LoyaltyPointTransactionConfiguration : IEntityTypeConfiguration<LoyaltyPointTransaction>
    {
        public void Configure(EntityTypeBuilder<LoyaltyPointTransaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Points)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(100);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.LoyaltyPointTransactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
