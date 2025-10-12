using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ProductThumbnailConfiguration : IEntityTypeConfiguration<ProductThumbnail>
{
    public void Configure(EntityTypeBuilder<ProductThumbnail> builder)
    {
        builder.HasKey(pt => new { pt.ImageId, pt.ProductId });

        builder.HasOne(pt => pt.Product)
            .WithOne(p => p.Thumbnail)
            .HasForeignKey<ProductThumbnail>(pt => pt.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.Image)
            .WithMany()
            .HasForeignKey(pt => pt.ImageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pt => pt.ImageOriginal)
            .WithMany()
            .HasForeignKey(pt => pt.ImageOriginalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasQueryFilter(pt => !pt.Product.IsDeleted);
    }
}