namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class ReplyPageViewModel
{
    public bool HasMoreReplies { get; set; }

    public ICollection<ProductCommentReplyViewModel> Replies { get; set; } = null!;
}