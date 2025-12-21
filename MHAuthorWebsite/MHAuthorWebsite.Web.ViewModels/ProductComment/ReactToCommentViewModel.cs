using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class ReactToCommentViewModel
{
    public Guid CommentId { get; set; }

    public CommentReaction ReactionType { get; set; }
}