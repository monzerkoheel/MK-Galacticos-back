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
    public class OrderShippingConfiguration : IEntityTypeConfiguration<OrderShipping>
    {
        public void Configure(EntityTypeBuilder<OrderShipping> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.OrderId)
                .IsUnique();

            // Shipping

            builder.Property(x => x.ShippingCost)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.EstimatedDeliveryDate)
                .IsRequired();

            builder.Property(x => x.DeliveredAt)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Address Snapshot

            builder.Property(x => x.RecipientName)
                .IsRequired(false)
                .HasMaxLength(150);

            builder.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Governorate)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Area)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.DetailedAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Notes)
                .IsRequired(false)
                .HasMaxLength(500);

            // Relationships

            builder.HasOne(x => x.Order)
                .WithOne(x => x.Shipping)
                .HasForeignKey<OrderShipping>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ShippingMethod)
                .WithMany(x => x.OrderShippings)
                .HasForeignKey(x => x.ShippingMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
