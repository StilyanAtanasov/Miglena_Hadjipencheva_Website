using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class ProductBaseCommentUserInfoDto
{
    public Guid Id { get; set; }

    public bool IsUserAuthor { get; set; }

    public CommentReaction? UserReaction { get; set; }

    public ICollection<ProductCommentReplyUserInfoDto> Replies { get; set; } = new HashSet<ProductCommentReplyUserInfoDto>();
}