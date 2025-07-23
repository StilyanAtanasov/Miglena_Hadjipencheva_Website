namespace MHAuthorWebsite.Core.Admin.Dto;

public class ImageUploadResultDto
{
    public string OriginalUrl { get; set; } = null!;

    public string PreviewUrl { get; set; } = null!;

    public string PublicId { get; set; } = null!;
}