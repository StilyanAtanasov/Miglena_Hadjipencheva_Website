using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ProductCommentImageConfiguration : IEntityTypeConfiguration<ProductCommentImage>
{
    public void Configure(EntityTypeBuilder<ProductCommentImage> builder)
    {
        builder
            .HasQueryFilter(reaction => !reaction.Comment.IsDeleted);
    }
}