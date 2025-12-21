using Microsoft.AspNetCore.Http;

namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class EditProductCommentDto
{
    public Guid CommentId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Guid? ReplyCommentId { get; set; }

    public short? Rating { get; set; }

    public string Text { get; set; } = null!;

    public ICollection<IFormFile>? NewImages { get; set; } = new HashSet<IFormFile>();

    public ICollection<EditProductCommentImageDto> ImagePreviewUrls { get; set; }
        = new HashSet<EditProductCommentImageDto>();

    public ICollection<string>? RemovedImagesUrls { get; set; } = new HashSet<string>();
}