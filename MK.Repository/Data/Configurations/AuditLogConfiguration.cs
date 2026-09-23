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
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.ActorType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.EntityId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.OldValues)
                .IsRequired(false);

            builder.Property(x => x.NewValues)
                .IsRequired(false);

            builder.Property(x => x.Description)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(x => x.IpAddress)
                .IsRequired(false)
                .HasMaxLength(45);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new
            {
                x.EntityName,
                x.EntityId
            });

            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.CreatedAt);
        }
    }
}
