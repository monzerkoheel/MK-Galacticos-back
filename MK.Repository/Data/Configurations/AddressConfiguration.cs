using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MK.Core.Models;


namespace MK.Repository.Data.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AddressName)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => new { x.UserId, x.AddressName })
                .IsUnique();

            builder.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Governorate)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Area)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.RecipientName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.DetailedAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
