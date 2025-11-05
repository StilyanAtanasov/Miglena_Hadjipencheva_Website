namespace MHAuthorWebsite.Core.Dto;

public class ProductCommentImagesUploadDto
{
    public ImageUploadResultDto Image { get; set; } = null!;

    public ImageUploadResultDto Preview { get; set; } = null!;
}
