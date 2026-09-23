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
    public class AvailabilityNotificationConfiguration : IEntityTypeConfiguration<AvailabilityNotification>
    {
        public void Configure(EntityTypeBuilder<AvailabilityNotification> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.IsSent)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.SentAt)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ProductSize)
                .IsRequired(false)
                .HasConversion<string>()
                .HasMaxLength(10);


            builder.HasOne(x => x.User)
                .WithMany(x => x.AvailabilityNotifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.AvailabilityNotifications)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.FanRequest)
                .WithMany(x => x.AvailabilityNotifications)
                .HasForeignKey(x => x.FanRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
