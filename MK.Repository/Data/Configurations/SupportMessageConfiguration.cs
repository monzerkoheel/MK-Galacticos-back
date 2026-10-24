using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MK.Core.Models;

namespace MK.Repository.Data.Configurations
{
    public class SupportMessageConfiguration : IEntityTypeConfiguration<SupportMessage>
    {
        public void Configure(EntityTypeBuilder<SupportMessage> builder)
        {
            builder.ToTable("SupportMessages");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.IsHandled)
                .HasDefaultValue(false);

            // SQL Server rowversion. If you use another database, remove this line and the RowVersion
            // property (and the DbUpdateConcurrencyException catch in SupportMessageService).
            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            // The sender (optional). Message stays if the user is removed.
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // The staff member who handled it. NoAction because SQL Server does not allow two SET NULL
            // paths from the same table to AspNetUsers (users are soft-deleted in this project anyway).
            builder.HasOne(x => x.HandledByUser)
                .WithMany()
                .HasForeignKey(x => x.HandledByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Listing: filter by IsHandled, newest first.
            builder.HasIndex(x => new { x.IsHandled, x.CreatedAt });
        }
    }
}
