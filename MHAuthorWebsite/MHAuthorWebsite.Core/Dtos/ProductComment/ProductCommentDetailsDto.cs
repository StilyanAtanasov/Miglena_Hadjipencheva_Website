using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class ProductCommentDetailsDto
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

    public ICollection<ProductCommentImageDto> Images { get; set; } = new HashSet<ProductCommentImageDto>();
}