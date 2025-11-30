using MHAuthorWebsite.Data.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class ProductCommentReplyUserInfoViewModel
{
    public Guid Id { get; set; }

    public CommentReaction? UserReaction { get; set; }

    public bool IsUserAuthor { get; set; }
}