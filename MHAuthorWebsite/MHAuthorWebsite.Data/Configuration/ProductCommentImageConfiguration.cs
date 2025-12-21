using MHAuthorWebsite.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ProductCommentImageConfiguration : IEntityTypeConfiguration<ProductCommentImage>
{
    public void Configure(EntityTypeBuilder<ProductCommentImage> builder)
    {
        builder
            .Property(image => image.PublicId)
            .HasComment("The publicId in Cloudinary");

        builder
            .Property(image => image.PreviewPublicId)
            .HasComment("The publicId for the image preview in Cloudinary");

        builder
            .HasQueryFilter(reaction => !reaction.Comment.IsDeleted);
    }
}