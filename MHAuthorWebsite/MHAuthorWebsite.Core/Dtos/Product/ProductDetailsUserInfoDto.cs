using MHAuthorWebsite.Core.Dtos.ProductComment;

namespace MHAuthorWebsite.Core.Dtos.Product;

public class ProductDetailsUserInfoDto
{
    public Guid CacheGeneralInfoCommitId { get; set; }

    public bool IsLiked { get; set; }

    public bool CanWriteMoreComments { get; set; }

    public bool IsRateLimitedForReplies { get; set; }

    public ICollection<ProductBaseCommentUserInfoDto> CommentSpecificInfo { get; set; } = new HashSet<ProductBaseCommentUserInfoDto>();
}