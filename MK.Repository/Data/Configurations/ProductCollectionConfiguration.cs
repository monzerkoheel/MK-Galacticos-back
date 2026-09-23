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
    public class ProductCollectionConfiguration : IEntityTypeConfiguration<ProductCollection>
    {
        public void Configure(EntityTypeBuilder<ProductCollection> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new
            {
                x.ProductId,
                x.CollectionId
            })
            .IsUnique();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductCollections)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Collection)
                .WithMany(x => x.ProductCollections)
                .HasForeignKey(x => x.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
