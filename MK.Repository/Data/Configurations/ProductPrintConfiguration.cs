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
    public class ProductPrintConfiguration : IEntityTypeConfiguration<ProductPrint>
    {
        public void Configure(EntityTypeBuilder<ProductPrint> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Number)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ProductId,
                x.Name,
                x.Number
            })
            .IsUnique();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Prints)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
