using MHAuthorWebsite.Data.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class ProductBaseCommentViewModel
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public short Rating { get; set; }

    public string Text { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime Date { get; set; }

    public bool VerifiedPurchase { get; set; }

    public int Likes { get; set; }

    public int Dislikes { get; set; }

    public CommentReaction? UserReaction { get; set; }

    public ICollection<string> ImageUrls { get; set; } = new HashSet<string>();

    public ICollection<ProductCommentReplyViewModel> Replies { get; set; } = new HashSet<ProductCommentReplyViewModel>();
}