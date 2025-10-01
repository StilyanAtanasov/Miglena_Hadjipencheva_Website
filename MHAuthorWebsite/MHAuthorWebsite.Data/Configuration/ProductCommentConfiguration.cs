using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ProductCommentConfiguration : IEntityTypeConfiguration<ProductComment>
{
    public void Configure(EntityTypeBuilder<ProductComment> builder)
    {
        builder
            .HasQueryFilter(c => !c.Product.IsDeleted && c.Product.IsPublic);
    }
}