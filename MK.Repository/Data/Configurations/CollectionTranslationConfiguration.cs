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
    public class CollectionTranslationConfiguration : IEntityTypeConfiguration<CollectionTranslation>
    {
        public void Configure(EntityTypeBuilder<CollectionTranslation> builder)
        {
            builder.ToTable("CollectionTranslations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Language)
                .HasConversion(new LanguageConverter())
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.HasOne(x => x.Collection)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.CollectionId,
                x.Language
            })
            .IsUnique();
        }
    }
}
