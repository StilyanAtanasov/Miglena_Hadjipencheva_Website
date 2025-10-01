using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ProductCommentReactionConfiguration : IEntityTypeConfiguration<ProductCommentReaction>
{
    public void Configure(EntityTypeBuilder<ProductCommentReaction> builder)
    {
        builder
            .HasQueryFilter(reaction => !reaction.Comment.IsDeleted);

        builder
            .HasOne(r => r.Comment)
            .WithMany(c => c.Reactions)
            .HasForeignKey(r => r.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(r => r.User)
            .WithMany(u => u.ProductCommentsReactions)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}