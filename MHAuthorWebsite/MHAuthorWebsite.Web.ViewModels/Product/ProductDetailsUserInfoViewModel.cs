using MHAuthorWebsite.Web.ViewModels.ProductComment;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductDetailsUserInfoViewModel
{
    public Guid CacheGeneralInfoCommitId { get; set; }

    public bool IsLiked { get; set; }

    public bool CanWriteMoreComments { get; set; }

    public bool IsRateLimitedForReplies { get; set; }

    public ICollection<ProductBaseCommentUserInfoViewModel> CommentSpecificInfo { get; set; } = new HashSet<ProductBaseCommentUserInfoViewModel>();
}