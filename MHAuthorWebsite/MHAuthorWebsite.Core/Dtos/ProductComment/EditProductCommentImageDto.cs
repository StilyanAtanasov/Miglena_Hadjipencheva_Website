namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class EditProductCommentImageDto
{
    public Guid ImageId { get; set; }

    public string PreviewUrl { get; set; } = null!;
}