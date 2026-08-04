using MHAuthorWebsite.Core.Dtos.Images;

namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class ProductCommentImagesUploadDto
{
    public ImageUploadResultDto Image { get; set; } = null!;

    public ImageUploadResultDto Preview { get; set; } = null!;
}
