namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class AddProductCommentDto
{
    public Guid ProductId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Guid? ReplyCommentId { get; set; }

    public short? Rating { get; set; }

    public string Text { get; set; } = null!;

    public string TargetName { get; set; } = null!;
}