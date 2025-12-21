namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class CommentPageDto
{
    public bool HasMoreComments { get; set; }

    public ICollection<ProductBaseCommentDto> Comments { get; set; } = null!;
}