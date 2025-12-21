using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class ProductBaseCommentUserInfoViewModel
{
    public Guid Id { get; set; }

    public bool IsUserAuthor { get; set; }

    public CommentReaction? UserReaction { get; set; }

    public ICollection<ProductCommentReplyUserInfoViewModel> Replies { get; set; } = new HashSet<ProductCommentReplyUserInfoViewModel>();
}