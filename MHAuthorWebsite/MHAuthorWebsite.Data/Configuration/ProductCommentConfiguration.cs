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


        builder
            .HasOne(pc => pc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(pc => pc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(pc => pc.ParentReply)
            .WithMany()
            .HasForeignKey(pc => pc.ParentReplyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}