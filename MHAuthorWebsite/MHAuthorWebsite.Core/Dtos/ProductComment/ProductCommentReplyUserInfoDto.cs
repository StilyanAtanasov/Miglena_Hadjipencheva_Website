using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class ProductCommentReplyUserInfoDto
{
    public Guid Id { get; set; }

    public CommentReaction? UserReaction { get; set; }

    public bool IsUserAuthor { get; set; }
}