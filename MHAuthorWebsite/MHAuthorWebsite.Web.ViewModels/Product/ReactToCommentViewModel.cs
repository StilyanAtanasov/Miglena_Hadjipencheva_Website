using MHAuthorWebsite.Data.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ReactToCommentViewModel
{
    public Guid CommentId { get; set; }

    public CommentReaction ReactionType { get; set; }
}