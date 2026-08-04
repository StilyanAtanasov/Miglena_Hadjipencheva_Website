namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class ReplyPageDto
{
    public bool HasMoreReplies { get; set; }

    public ICollection<ProductCommentReplyDto> Replies { get; set; } = null!;
}