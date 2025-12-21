namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class ProductBaseCommentGeneralInfoDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public short Rating { get; set; }

    public string Text { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime Date { get; set; }

    public DateTime? LastEdited { get; set; }

    public bool VerifiedPurchase { get; set; }

    public int Likes { get; set; }

    public int Dislikes { get; set; }

    public bool HasMoreReplies { get; set; }

    public int TotalRepliesCount { get; set; }

    public ICollection<string> ImageUrls { get; set; } = new HashSet<string>();

    public ICollection<ProductCommentReplyGeneralInfoDto> Replies { get; set; } = new HashSet<ProductCommentReplyGeneralInfoDto>();
}