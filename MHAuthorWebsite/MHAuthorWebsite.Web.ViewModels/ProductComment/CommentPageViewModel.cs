namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class CommentPageViewModel
{
    public bool HasMoreComments { get; set; }

    public ICollection<ProductBaseCommentViewModel> Comments { get; set; } = null!;
}